using Xunit;

namespace IndustrialPress.IotTelemetry.UnitTests;

public class TelemetryRedisKeyTests
{
    [Theory]
    [InlineData(1, "telemetry:sensor:1")]
    [InlineData(20, "telemetry:sensor:20")]
    public void Redis_key_format_matches_architecture(int sensorId, string expected) =>
        Assert.Equal(expected, $"telemetry:sensor:{sensorId}");
}
