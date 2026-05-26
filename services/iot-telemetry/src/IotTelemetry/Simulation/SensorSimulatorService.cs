using IndustrialPress.IotTelemetry.Infrastructure;
using IndustrialPress.IotTelemetry.Models;

namespace IndustrialPress.IotTelemetry.Simulation;

public sealed class SensorSimulatorService(
    RedisTelemetryWriter redisWriter,
    RabbitMqTelemetryPublisher rabbitPublisher,
    IConfiguration configuration,
    ILogger<SensorSimulatorService> logger) : BackgroundService
{
    private readonly int _sensorCount = configuration.GetValue("Sensor:Count", 20);
    private readonly int _intervalSeconds = configuration.GetValue("Sensor:IntervalSeconds", 1);
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await rabbitPublisher.EnsureInfrastructureAsync(stoppingToken);
        logger.LogInformation("IoT simulator started: {Count} sensors, interval {Interval}s", _sensorCount, _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var tickStart = DateTimeOffset.UtcNow;
            var tasks = Enumerable.Range(1, _sensorCount)
                .Select(id => PublishSensorTickAsync(id, stoppingToken));
            await Task.WhenAll(tasks);

            var elapsed = DateTimeOffset.UtcNow - tickStart;
            var delay = TimeSpan.FromSeconds(_intervalSeconds) - elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task PublishSensorTickAsync(int sensorId, CancellationToken cancellationToken)
    {
        var sample = new TelemetrySample(
            sensorId,
            Math.Round(20 + _random.NextDouble() * 15, 2),
            "celsius",
            DateTimeOffset.UtcNow,
            "ok");

        if (!await redisWriter.WriteWithRetryAsync(sample, cancellationToken))
            return;

        await rabbitPublisher.PublishWithRetryAsync(sample, cancellationToken);
    }
}
