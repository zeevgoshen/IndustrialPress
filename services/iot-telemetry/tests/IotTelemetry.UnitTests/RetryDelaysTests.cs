using IndustrialPress.IotTelemetry.Infrastructure;
using Xunit;

namespace IndustrialPress.IotTelemetry.UnitTests;

public class RetryDelaysTests
{
    [Fact]
    public void IoT_publish_backoff_has_three_attempts() =>
        Assert.Equal(3, RetryDelays.IoTPublishBackoffMs.Length);

    [Fact]
    public async Task DelayBeforeAttempt_does_not_delay_on_first_try()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await RetryDelays.DelayBeforeAttempt(0, RetryDelays.IoTPublishBackoffMs, CancellationToken.None);
        Assert.True(sw.ElapsedMilliseconds < 50);
    }
}
