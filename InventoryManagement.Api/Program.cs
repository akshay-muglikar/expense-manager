using System.Text;
using InventoryManagement.Api;
using InventoryManagement.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Database");

// Add MySQL DbContext (if using EF Core)

builder.Services.AddDbContext<ApplicationDbContext>(
    options => {
            options.UseMySql(connectionString,
            ServerVersion.AutoDetect(connectionString));
            DbContextOptions<ApplicationDbContext> op = (DbContextOptions<ApplicationDbContext>)options.Options;
        ApplicationDbContext applicationDbContext = new ApplicationDbContext(op);
        applicationDbContext.Database.Migrate();
    }
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5050",
            ValidAudience = "http://localhost:4200",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JwtSettings:SecretKeyNew123JwtSettings:SecretKeyNew123"))
        };
    });
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(Program)); // Register AutoMapper
builder.Services.AddControllers();
// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDomainDependencies();
builder.Services.AddApiDependencies();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        });
});
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
app.UseCors("AllowAllOrigins");
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();
app.Run();
