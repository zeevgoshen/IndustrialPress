using System.Text.Json;
using IndustrialPress.RestApi.Models;
using StackExchange.Redis;

namespace IndustrialPress.RestApi.Infrastructure;

public sealed class RedisTelemetryStore(IConnectionMultiplexer redis, ILogger<RedisTelemetryStore> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<TelemetryPayload?> GetWithRetryAsync(int sensorId, CancellationToken cancellationToken)
    {
        int[] backoff = [50, 150];
        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(backoff[attempt - 1], cancellationToken);

            try
            {
                var value = await _db.StringGetAsync($"telemetry:sensor:{sensorId}");
                if (!value.HasValue) return null;
                return JsonSerializer.Deserialize<TelemetryPayload>(value.ToString(), JsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "redis_read_failed sensorId={SensorId} attempt={Attempt}", sensorId, attempt + 1);
            }
        }

        logger.LogError("redis_read_failed sensorId={SensorId}", sensorId);
        return null;
    }

    public async Task<IReadOnlyList<TelemetryPayload>> GetAllSensorsSnapshotAsync(int sensorCount, CancellationToken cancellationToken)
    {
        var list = new List<TelemetryPayload>();
        for (var id = 1; id <= sensorCount; id++)
        {
            var payload = await GetWithRetryAsync(id, cancellationToken);
            if (payload is not null)
                list.Add(payload);
        }
        return list;
    }
}
