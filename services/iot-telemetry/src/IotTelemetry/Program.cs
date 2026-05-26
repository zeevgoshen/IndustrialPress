using IndustrialPress.IotTelemetry.Infrastructure;
using IndustrialPress.IotTelemetry.Simulation;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddSingleton<RedisTelemetryWriter>();
builder.Services.AddSingleton<RabbitMqTelemetryPublisher>();
builder.Services.AddHostedService<SensorSimulatorService>();

var host = builder.Build();
host.Run();
