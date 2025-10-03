using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai;
using NTSkelbimuSistemaSaitynai.DbUtils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string connString = new DbConnection().GetConnectionString();
builder.Services.AddDbContext<PostgresContext>(options => options.UseNpgsql(connString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
