using IndustrialPress.RestApi.Hubs;
using IndustrialPress.RestApi.Infrastructure;
using IndustrialPress.RestApi.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddSingleton<RedisTelemetryStore>();
builder.Services.AddSingleton<SensorMetadataGateway>();
builder.Services.AddHostedService<TelemetryRabbitMqConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () => Results.Ok(new { service = "rest-api", status = "healthy" }));

app.MapGet("/api/sensors", async (SensorMetadataGateway sql, CancellationToken ct) =>
{
    try
    {
        var sensors = await sql.GetSensorsAsync(ct);
        return Results.Ok(sensors.Select(s => new
        {
            s.Id,
            s.Name,
            s.Location,
            s.Type,
            s.Enabled
        }));
    }
    catch
    {
        return Results.Json(new { error = "SQL Data service unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/api/sensors/{id:int}", async (int id, SensorMetadataGateway sql, CancellationToken ct) =>
{
    try
    {
        var sensor = await sql.GetSensorAsync(id, ct);
        return sensor is null ? Results.NotFound() : Results.Ok(sensor);
    }
    catch
    {
        return Results.Json(new { error = "SQL Data service unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapHub<TelemetryHub>("/hubs/telemetry");

app.Run();
