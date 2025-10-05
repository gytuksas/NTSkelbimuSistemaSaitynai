using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NTSkelbimuSistemaSaitynai.DbUtils;
using NTSkelbimuSistemaSaitynai;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string connString = new DbConnection().GetConnectionString();
builder.Services.AddDbContext<PostgresContext>(options => options.UseNpgsql(connString));

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
