using CarDealership.Api.Data;
using CarDealership.Api.Persistence;
using CarDealership.Api.Security;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CarDealership.Api.Filters;

/// <summary>
/// CarDealership API Startup Configuration
/// Sets up services, authentication, authorization, and middleware pipeline
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// Configure application settings from appsettings.json
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<OtpOptions>(builder.Configuration.GetSection("Otp"));

// Configure Entity Framework with SQLite database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Register application services (business logic layer)
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddHttpContextAccessor();

// Configure JWT authentication
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

// Enable role-based authorization
builder.Services.AddAuthorization();

// Configure controllers with input sanitization filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TrimAndNormalizeFilter>(); // Automatically trims strings & lowercases Email fields
});

// Configure Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerConfig =>
{
    swaggerConfig.SwaggerDoc("v1", new OpenApiInfo { Title = "Car Dealership API", Version = "v1" });

    // Configure JWT Bearer authentication for Swagger
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    };

    swaggerConfig.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    // Require JWT authentication for all endpoints in Swagger
    swaggerConfig.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize database with sample data (migrations + seeding)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitAsync(dbContext);
}

// Configure development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(swaggerUIConfig =>
    {
        // Persist JWT authorization across browser refreshes
        swaggerUIConfig.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

// Configure middleware pipeline (order matters!)
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.Run();
