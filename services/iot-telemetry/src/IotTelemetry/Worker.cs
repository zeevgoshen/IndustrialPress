namespace IndustrialPress.IotTelemetry;

/// <summary>
/// Phase 3: simulate 20 sensors @ 1 Hz, write Redis (R1), publish RabbitMQ (R2).
/// See docs/architecture.md
/// </summary>
public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    private const int SensorCount = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("IoT Telemetry service started (Phase 0 placeholder, {Count} sensors planned)", SensorCount);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            logger.LogDebug("Heartbeat — implement sensor loop in Phase 3");
        }
    }
}
