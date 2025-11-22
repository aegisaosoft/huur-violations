/*
 *
 * Copyright (c) 2025 Alexander Orlov.
 * 34 Middletown Ave Atlantic Highlands NJ 07716
 *
 * THIS SOFTWARE IS THE CONFIDENTIAL AND PROPRIETARY INFORMATION OF
 * Alexander Orlov. ("CONFIDENTIAL INFORMATION"). YOU SHALL NOT DISCLOSE
 * SUCH CONFIDENTIAL INFORMATION AND SHALL USE IT ONLY IN ACCORDANCE
 * WITH THE TERMS OF THE LICENSE AGREEMENT YOU ENTERED INTO WITH
 * Alexander Orlov.
 *
 * Author: Alexander Orlov
 *
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarRental.Api.Data;
using CarRental.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Car Rental API",
        Version = "v1",
        Description = "API for managing car rental operations with multi-company support, Stripe integration, and comprehensive rental management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Car Rental API Support",
            Email = "support@carrentalapi.com"
        }
    });
    
    // Add JWT Bearer authorization to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Database Configuration Service
builder.Services.AddScoped<IDatabaseConfigService, DatabaseConfigService>();

// Add Entity Framework with configuration
builder.Services.AddDbContext<CarRentalDbContext>((serviceProvider, options) =>
{
    var dbConfigService = serviceProvider.GetRequiredService<IDatabaseConfigService>();
    var connectionString = dbConfigService.GetConnectionString();
    var dbSettings = dbConfigService.GetDatabaseSettings();

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(dbSettings.CommandTimeout);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: dbSettings.MaxRetryCount,
            maxRetryDelay: TimeSpan.Parse(dbSettings.MaxRetryDelay),
            errorCodesToAdd: null);
    });

    if (dbSettings.EnableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }

    if (dbSettings.EnableDetailedErrors)
    {
        options.EnableDetailedErrors();
    }

    if (dbSettings.EnableServiceProviderCaching)
    {
        options.EnableServiceProviderCaching();
    }

    // Query splitting behavior is handled at query level, not configuration level
    // This setting is for reference only
});

// Add JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-jwt-key-that-should-be-at-least-32-characters-long-for-production-use";
var issuer = jwtSettings["Issuer"] ?? "CarRentalAPI";
var audience = jwtSettings["Audience"] ?? "CarRentalClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger for all environments (public API)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Rental API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();