using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NTSkelbimuSistemaSaitynai;
using NTSkelbimuSistemaSaitynai.Configuration;
using NTSkelbimuSistemaSaitynai.DbUtils;
using NuGet.Protocol.Plugins;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter>();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string connString = new DbConnection().GetConnectionString();
builder.Services.AddDbContext<PostgresContext>(options => options.UseNpgsql(connString));

var uploadsRoot = builder.Configuration.GetValue<string>($"{FileStorageOptions.SectionName}:UploadRoot");
if (string.IsNullOrWhiteSpace(uploadsRoot))
{
    uploadsRoot = Path.Combine(builder.Environment.ContentRootPath, "storage", "uploads");
}
else if (!Path.IsPathRooted(uploadsRoot))
{
    uploadsRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, uploadsRoot));
}

var requestPath = builder.Configuration.GetValue<string>($"{FileStorageOptions.SectionName}:RequestPath") ?? "/uploads";

builder.Services.Configure<FileStorageOptions>(options =>
{
    options.UploadRoot = uploadsRoot;
    options.RequestPath = string.IsNullOrWhiteSpace(requestPath) ? "/uploads" : requestPath;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NTSkelbimuSistemaSaitynai API",
        Version = "v1",
        Description = "OpenAPI specification for the NTSkelbimuSistemaSaitynai service"
    });

    // Include XML comments if available (enable GenerateDocumentationFile in csproj)
    var xmlFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

Configuration configuration = Configuration.GetConfiguration();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Account-Unconfirmed", "X-Account-Blocked"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.Jwt.Issuer,
            ValidAudience = configuration.Jwt.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Jwt.Key))
        };
    });

builder.Services.AddAuthorization();

// Custom filters/services
builder.Services.AddScoped<NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter>();
builder.Services.AddScoped<NTSkelbimuSistemaSaitynai.Authorization.OwnershipService>();

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
var app = builder.Build();

var fileStorageOptions = app.Services.GetRequiredService<IOptions<FileStorageOptions>>().Value;
Directory.CreateDirectory(fileStorageOptions.UploadRoot);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Protect Swagger UI with HTTP Basic Auth in non-Development environments
if (!app.Environment.IsDevelopment())
{
    // Try to load credentials from a Docker secret file first (path from env: SWAGGER_AUTH_FILE).
    string? swaggerUser = null;
    string? swaggerPass = null;

    string? secretPath = builder.Configuration["SWAGGER_AUTH_FILE"];
    // If not explicitly set, use common default path if the file exists
    if (string.IsNullOrWhiteSpace(secretPath))
    {
        const string defaultSecretPath = "/run/secrets/swaggerauth";
        if (System.IO.File.Exists(defaultSecretPath))
        {
            secretPath = defaultSecretPath;
        }
    }

    if (!string.IsNullOrWhiteSpace(secretPath) && System.IO.File.Exists(secretPath))
    {
        try
        {
            var content = await System.IO.File.ReadAllTextAsync(secretPath);
            content = content.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                if (content.StartsWith("{"))
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("Username", out var uProp))
                    {
                        swaggerUser = uProp.GetString();
                    }
                    if (doc.RootElement.TryGetProperty("Password", out var pProp))
                    {
                        swaggerPass = pProp.GetString();
                    }
                }
                else if (content.Contains(':'))
                {
                    var parts = content.Split(':', 2);
                    swaggerUser = parts[0].Trim();
                    swaggerPass = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                }
                else
                {
                    var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length >= 2)
                    {
                        swaggerUser = lines[0].Trim();
                        swaggerPass = lines[1].Trim();
                    }
                }
            }
        }
        catch
        {
            // Ignore parse errors; we'll fall back to config/defaults
        }
    }

    // Fall back to appsettings or env vars if secret not supplied or invalid
    swaggerUser ??= builder.Configuration["SwaggerAuth:Username"];
    swaggerPass ??= builder.Configuration["SwaggerAuth:Password"];
    
    // Validate that credentials are configured in non-development environments
    if (string.IsNullOrEmpty(swaggerUser) || string.IsNullOrEmpty(swaggerPass))
    {
        throw new InvalidOperationException("Swagger authentication credentials must be configured in non-development environments. " +
            "Set credentials via Docker secrets (SWAGGER_AUTH_FILE), appsettings.json (SwaggerAuth:Username/Password), or environment variables.");
    }

    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger UI\"";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authentication required.");
                return;
            }

            try
            {
                var encoded = authHeader["Basic ".Length..].Trim();
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = decoded.Split(':', 2);
                var user = parts[0];
                var pass = parts.Length > 1 ? parts[1] : string.Empty;

                // Use a constant-time comparison to prevent timing attacks
                if (!string.Equals(user, swaggerUser, StringComparison.OrdinalIgnoreCase) || 
                    !CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(pass ?? string.Empty),
                        Encoding.UTF8.GetBytes(swaggerPass ?? string.Empty)))
                {
                    context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger UI\"";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid credentials.");
                    return;
                }
            }
            catch
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger UI\"";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid authentication header.");
                return;
            }
        }

        await next();
    });
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fileStorageOptions.UploadRoot),
    RequestPath = fileStorageOptions.RequestPath
});
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseMvc();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
