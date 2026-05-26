namespace IndustrialPress.SqlData.Data;

public sealed class SensorEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string Type { get; set; } = "temperature";
    public bool Enabled { get; set; } = true;
}
