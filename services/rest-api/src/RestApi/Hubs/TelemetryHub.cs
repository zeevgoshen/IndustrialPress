using IndustrialPress.RestApi.Infrastructure;
using IndustrialPress.RestApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace IndustrialPress.RestApi.Hubs;

public sealed class TelemetryHub(RedisTelemetryStore redisStore, IConfiguration configuration) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var count = configuration.GetValue("Sensor:Count", 20);
        var snapshot = await redisStore.GetAllSensorsSnapshotAsync(count, Context.ConnectionAborted);
        await Clients.Caller.SendAsync("TelemetrySnapshot", snapshot, Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }

}
