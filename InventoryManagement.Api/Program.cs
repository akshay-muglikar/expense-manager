using InventoryManagement.Api;
using InventoryManagement.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(
    options => {
        options.UseSqlite("Data Source=Test.db");
        DbContextOptions<ApplicationDbContext> op = (DbContextOptions<ApplicationDbContext>)options.Options;
        ApplicationDbContext applicationDbContext = new ApplicationDbContext(op);
        applicationDbContext.Database.Migrate();
    }
);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDomainDependencies();
builder.Services.AddApiDependencies();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Example",
        Version = "v1"
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
