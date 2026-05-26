using IndustrialPress.RestApi.Hubs;
using IndustrialPress.RestApi.Infrastructure;
using IndustrialPress.RestApi.Services;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IndustrialPress REST API",
        Version = "v1",
        Description =
            "Public REST surface for the UI. Sensor **metadata** only. " +
            "Live telemetry uses **SignalR** at `/hubs/telemetry` (not listed here). " +
            "Backend-to-backend traffic uses gRPC and RabbitMQ."
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddSingleton<RedisTelemetryStore>();
builder.Services.AddSingleton<SensorMetadataGateway>();
builder.Services.AddHostedService<TelemetryRabbitMqConsumer>();

var app = builder.Build();

// Swagger enabled for local/docker demo (telemetry still via SignalR only)
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "IndustrialPress REST API v1"));

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");

app.Run();
