using System.Text.Json;
using IndustrialPress.IotTelemetry.Models;
using StackExchange.Redis;

namespace IndustrialPress.IotTelemetry.Infrastructure;

public sealed class RedisTelemetryWriter(IConnectionMultiplexer redis, ILogger<RedisTelemetryWriter> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<bool> WriteWithRetryAsync(TelemetrySample sample, CancellationToken cancellationToken)
    {
        var key = $"telemetry:sensor:{sample.SensorId}";
        var json = JsonSerializer.Serialize(new
        {
            sensorId = sample.SensorId,
            value = sample.Value,
            unit = sample.Unit,
            timestamp = sample.Timestamp.UtcDateTime,
            status = sample.Status
        }, JsonOptions);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await RetryDelays.DelayBeforeAttempt(attempt, RetryDelays.IoTPublishBackoffMs, cancellationToken);
            try
            {
                if (await _db.StringSetAsync(key, json))
                    return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "redis_write_failed sensorId={SensorId} attempt={Attempt}", sample.SensorId, attempt + 1);
            }
        }

        logger.LogError("redis_write_failed sensorId={SensorId} exhausted retries", sample.SensorId);
        return false;
    }
}
