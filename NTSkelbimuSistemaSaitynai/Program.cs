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
              .AllowAnyMethod());
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
