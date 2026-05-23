// src/WebApi/Program.cs
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using WebApi.Middlewares;

// src/WebApi/Program.cs (agregar health checks)

using WebApi.HealthChecks;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Agregar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configurar Redis para Health Check
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<RedisHealthCheck>("redis")
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

app.UseMiddleware<RateLimitingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();