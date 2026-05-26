namespace IndustrialPress.RestApi.Dtos;

/// <summary>Sensor metadata from SQL Data service (not live telemetry).</summary>
public sealed class SensorMetadataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string Type { get; set; } = "";
    public bool Enabled { get; set; }
}
