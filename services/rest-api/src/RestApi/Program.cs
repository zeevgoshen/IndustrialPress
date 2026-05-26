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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var redis = config.GetConnectionString("Redis") ?? "localhost:6379";
    var options = ConfigurationOptions.Parse(redis);
    options.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddSingleton<RedisTelemetryStore>();
builder.Services.AddSingleton<ISensorMetadataGateway, SensorMetadataGateway>();
builder.Services.AddHostedService<TelemetryRabbitMqConsumer>();

var app = builder.Build();

// Swagger enabled for local/docker demo (telemetry still via SignalR only)
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "IndustrialPress REST API v1"));

app.UseCors();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");

app.Run();

public partial class Program;
