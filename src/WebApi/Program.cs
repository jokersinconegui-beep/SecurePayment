// src/WebApi/Program.cs
using Application;
using Infrastructure;
using WebApi.Middlewares;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.OpenApi.Models;
using Infrastructure.Persistence;
using Infrastructure.Services;
using WebApi.RateLimiters;

var builder = WebApplication.CreateBuilder(args);

// Agregar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPrometheusMetrics();  // ✅ Nuevo nombre, sin ambigüedad

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
Log.Information("=== SERILOG IS WORKING ===");

// Configurar autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SecurePaymentGatewaySuperSecretKey2026_AtLeast32Chars";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                    context.Token = token["Bearer ".Length..].Trim();

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddSingleton<MerchantRateLimiter>();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "SecurePaymentGateway API", 
        Version = "v1",
        Description = "API para procesamiento de pagos seguros con rate limiting por merchant"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

var app = builder.Build();

// Crear base de datos
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Configurar métricas con nuevo nombre
app.UsePrometheusMetrics();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0}ms";
    options.GetLevel = (httpContext, elapsed, ex) => 
        httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<MerchantRateLimitingMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();