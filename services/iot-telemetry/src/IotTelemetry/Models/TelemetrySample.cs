namespace IndustrialPress.IotTelemetry.Models;

public sealed record TelemetrySample(
    int SensorId,
    double Value,
    string Unit,
    DateTimeOffset Timestamp,
    string Status);
