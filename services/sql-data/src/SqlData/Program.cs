using IndustrialPress.Contracts.Sensors;
using static IndustrialPress.Contracts.Sensors.SensorMetadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
// Phase 2+: AddDbContext, SensorMetadataGrpcService — see docs/architecture.md

var app = builder.Build();

app.MapGrpcService<PlaceholderSensorMetadataService>();
app.MapGet("/health", () => Results.Ok(new { service = "sql-data", status = "healthy", phase = 0 }));

app.Run();

/// <summary>Phase 2: replace with EF-backed implementation.</summary>
internal sealed class PlaceholderSensorMetadataService : SensorMetadataBase
{
    public override Task<GetSensorsResponse> GetSensors(GetSensorsRequest request, Grpc.Core.ServerCallContext context)
    {
        var response = new GetSensorsResponse();
        for (var id = 1; id <= 20; id++)
        {
            response.Sensors.Add(new Sensor
            {
                Id = id,
                Name = $"Sensor-{id:D2}",
                Location = "Line-1",
                Type = "temperature",
                Enabled = true
            });
        }
        return Task.FromResult(response);
    }

    public override Task<Sensor> GetSensor(GetSensorRequest request, Grpc.Core.ServerCallContext context)
    {
        return Task.FromResult(new Sensor
        {
            Id = request.Id,
            Name = $"Sensor-{request.Id:D2}",
            Location = "Line-1",
            Type = "temperature",
            Enabled = request.Id is >= 1 and <= 20
        });
    }
}
