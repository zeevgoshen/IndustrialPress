using IndustrialPress.Contracts.Sensors;

namespace IndustrialPress.RestApi.Services;

public interface ISensorMetadataGateway
{
    Task<IReadOnlyList<Sensor>> GetSensorsAsync(CancellationToken cancellationToken);
    Task<Sensor?> GetSensorAsync(int id, CancellationToken cancellationToken);
}
