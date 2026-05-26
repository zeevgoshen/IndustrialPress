using Grpc.Core;
using IndustrialPress.Contracts.Sensors;
using IndustrialPress.SqlData.Data;
using Microsoft.EntityFrameworkCore;
using static IndustrialPress.Contracts.Sensors.SensorMetadata;

namespace IndustrialPress.SqlData.Grpc;

public sealed class SensorMetadataGrpcService(IndustrialDbContext db) : SensorMetadataBase
{
    public override async Task<GetSensorsResponse> GetSensors(
        GetSensorsRequest request,
        ServerCallContext context)
    {
        var entities = await db.Sensors.AsNoTracking().OrderBy(s => s.Id).ToListAsync(context.CancellationToken);
        var response = new GetSensorsResponse();
        response.Sensors.AddRange(entities.Select(ToProto));
        return response;
    }

    public override async Task<Sensor> GetSensor(GetSensorRequest request, ServerCallContext context)
    {
        var entity = await db.Sensors.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, context.CancellationToken);

        if (entity is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Sensor {request.Id} not found"));

        return ToProto(entity);
    }

    private static Sensor ToProto(SensorEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Location = e.Location,
        Type = e.Type,
        Enabled = e.Enabled
    };
}
