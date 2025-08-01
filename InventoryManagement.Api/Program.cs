using System.Text;
using InventoryManagement.Api;
using InventoryManagement.Api.Config;
using InventoryManagement.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Database");
var userDatabaseConnectionString = builder.Configuration.GetConnectionString("UserDatabase");
builder.Services.Configure<InventoryConfig>(builder.Configuration);

builder.Services.AddHttpContextAccessor();

// Add MySQL DbContext (if using EF Core)
builder.Services.AddDbContext<IdentityDbContext>(
    options => {
        options.UseSqlite(userDatabaseConnectionString); 
        DbContextOptions<IdentityDbContext> op = (DbContextOptions<IdentityDbContext>)options.Options;
        IdentityDbContext applicationDbContext = new IdentityDbContext(op);
        //applicationDbContext.Database.EnsureCreated();
        applicationDbContext.Database.Migrate();
    }
);

builder.Services.AddDbContext<ApplicationDbContext>(ConfigureApplicationDbContext);

void ConfigureApplicationDbContext(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
{
    // get clientid from url
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    // Get the clientId from the path parameter

    var clientId = httpContext?.User?.FindFirst("client_id")?.Value;
    if(httpContext?.Request?.RouteValues?.TryGetValue("clientId", out var clientIdValue)??false){
        clientId = clientIdValue.ToString();
    }
    var clientIdConnectionString = connectionString.Replace("{clientId}", clientId);
    if(clientId!=null)
    {
        options.UseSqlite(clientIdConnectionString);
        DbContextOptions<ApplicationDbContext> op = (DbContextOptions<ApplicationDbContext>)options.Options;
        ApplicationDbContext applicationDbContext = new ApplicationDbContext(op);
        //applicationDbContext.Database.EnsureCreated();
        applicationDbContext.Database.Migrate();
    }
}

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
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
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
