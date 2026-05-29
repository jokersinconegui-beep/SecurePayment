// src/WebApi/Program.cs
using Application;
using Infrastructure;
using WebApi.Middlewares;

// src/WebApi/Program.cs (agregar health checks)
using Serilog;
using Serilog.Events;
using WebApi.HealthChecks;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Common.Interfaces;
using Infrastructure.Services.Persistence;
using Microsoft.OpenApi.Models;
using Infrastructure.Persistence;
using Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

// Agregar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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

        // Para que funcione con Swagger
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
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configurar Redis para Health Check
// var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
// builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//     ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddEndpointsApiExplorer();
// src/WebApi/Program.cs (mejorar Swagger)


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SecurePaymentGateway API", Version = "v1" });

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
    // .AddCheck<RedisHealthCheck>("redis")
    .AddDbContextCheck<ApplicationDbContext>("database");

var app = builder.Build();

// Crear y migrar base de datos automáticamente
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
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0}ms";
    options.GetLevel = (httpContext, elapsed, ex) => 
        httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information;
});

app.UseMiddleware<RateLimitingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();





