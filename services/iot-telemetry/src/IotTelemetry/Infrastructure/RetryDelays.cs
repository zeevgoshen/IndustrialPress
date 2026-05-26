namespace IndustrialPress.IotTelemetry.Infrastructure;

internal static class RetryDelays
{
    public static readonly int[] IoTPublishBackoffMs = [100, 250, 500];

    public static async Task DelayBeforeAttempt(int attempt, int[] backoffMs, CancellationToken cancellationToken)
    {
        if (attempt > 0)
            await Task.Delay(backoffMs[Math.Min(attempt - 1, backoffMs.Length - 1)], cancellationToken);
    }
}
