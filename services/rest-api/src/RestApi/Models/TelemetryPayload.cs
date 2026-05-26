namespace IndustrialPress.RestApi.Models;

public sealed class TelemetryPayload
{
    public int SensorId { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = "ok";
}
