using Grpc.Net.Client;
using IndustrialPress.Contracts.Sensors;
using GrpcSensorClient = IndustrialPress.Contracts.Sensors.SensorMetadata.SensorMetadataClient;

namespace IndustrialPress.RestApi.Services;

public sealed class SensorMetadataGateway(IConfiguration configuration, ILogger<SensorMetadataGateway> logger)
{
    private readonly string _address = configuration["Grpc:SqlData"] ?? "http://localhost:5102";

    public async Task<IReadOnlyList<Sensor>> GetSensorsAsync(CancellationToken cancellationToken)
    {
        using var channel = CreateChannel();
        var client = new GrpcSensorClient(channel);
        var response = await client.GetSensorsAsync(new GetSensorsRequest(), cancellationToken: cancellationToken);
        return response.Sensors;
    }

    public async Task<Sensor?> GetSensorAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using var channel = CreateChannel();
            var client = new GrpcSensorClient(channel);
            return await client.GetSensorAsync(new GetSensorRequest { Id = id }, cancellationToken: cancellationToken);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "grpc_sql_failed sensorId={SensorId}", id);
            throw;
        }
    }

    private GrpcChannel CreateChannel()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        return GrpcChannel.ForAddress(_address);
    }
}
