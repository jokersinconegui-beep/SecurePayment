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
using WebApi.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Agregar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPrometheusMetrics();  // Métricas desde Infrastructure

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

// Configurar Redis (si está disponible, opcional)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
try
{
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    Log.Information("✅ Redis connected successfully");
}
catch (Exception ex)
{
    Log.Warning($"⚠️ Redis not available: {ex.Message}. Continuing without Redis cache.");
    // builder.Services.AddSingleton<IConnectionMultiplexer>(null!);
}

// Configurar Swagger con JWT
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


// Health Checks avanzados (después de builder.Services)
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready", "live"])
    .AddCheck<MemoryHealthCheck>("memory", tags: ["ready"])
    .AddDiskStorageHealthCheck(setup =>
    {
        setup.AddDrive(Path.GetPathRoot(Directory.GetCurrentDirectory()) ?? "C:\\", 
            minimumFreeMegabytes: 1024);
    }, tags: ["ready"]);

// Registrar Redis health check por separado después de Build

// Configurar opciones de health checks
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = _ => true;
});

var app = builder.Build();

// Crear base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    Log.Information("✅ Database ensured created");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar métricas
app.UsePrometheusMetrics();

// Middleware de logging de requests
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

// ✅ Health Check endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live"),
    ResponseWriter = WriteHealthCheckResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

app.Run();

// Función auxiliar para formatear respuesta JSON de health checks
static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    
    var result = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.TotalMilliseconds,
            data = e.Value.Data
        }),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        timestamp = DateTime.UtcNow
    };
    
    return context.Response.WriteAsJsonAsync(result);
}