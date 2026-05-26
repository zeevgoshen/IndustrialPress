using IndustrialPress.Contracts.Sensors;
using IndustrialPress.RestApi.Services;

namespace IndustrialPress.RestApi.UnitTests;

internal sealed class FakeSensorMetadataGateway : ISensorMetadataGateway
{
    private readonly IReadOnlyList<Sensor> _sensors;

    public FakeSensorMetadataGateway()
    {
        _sensors = Enumerable.Range(1, 20).Select(id => new Sensor
        {
            Id = id,
            Name = $"Sensor-{id:D2}",
            Location = "Line-1",
            Type = "temperature",
            Enabled = true
        }).ToList();
    }

    public Task<IReadOnlyList<Sensor>> GetSensorsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_sensors);

    public Task<Sensor?> GetSensorAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_sensors.FirstOrDefault(s => s.Id == id));
}
