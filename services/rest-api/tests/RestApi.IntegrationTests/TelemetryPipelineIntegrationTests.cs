using System.Collections.Concurrent;
using System.Text.Json;
using IndustrialPress.IotTelemetry.Infrastructure;
using IndustrialPress.IotTelemetry.Models;
using IndustrialPress.RestApi.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace IndustrialPress.RestApi.IntegrationTests;

[Collection(IntegrationCollection.Name)]
public sealed class TelemetryPipelineIntegrationTests(IntegrationTestFixture factory)
{
    private sealed class TelemetryMessage
    {
        public int SensorId { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "";
    }

    [Fact]
    public async Task All_twenty_sensors_flow_through_redis_rabbitmq_and_signalr()
    {
        var received = new ConcurrentDictionary<int, byte>();
        var hubUrl = new Uri(factory.Server.BaseAddress!, "/hubs/telemetry");

        await using var hub = new HubConnectionBuilder()
            .WithUrl(hubUrl, o => o.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
            .WithAutomaticReconnect()
            .Build();

        hub.On<TelemetryMessage>("TelemetryUpdated", msg =>
        {
            received[msg.SensorId] = 1;
            return Task.CompletedTask;
        });

        hub.On<TelemetryMessage[]>("TelemetrySnapshot", snapshot =>
        {
            foreach (var msg in snapshot)
                received[msg.SensorId] = 1;
            return Task.CompletedTask;
        });

        await hub.StartAsync();

        var iotConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = factory.RedisConnectionString,
                ["RabbitMQ:Host"] = factory.RabbitMqHost,
                ["RabbitMQ:Port"] = factory.RabbitMqPort.ToString(),
                ["RabbitMQ:User"] = "industrial",
                ["RabbitMQ:Password"] = "industrial"
            })
            .Build();

        var redis = await ConnectionMultiplexer.ConnectAsync(factory.RedisConnectionString);
        var writer = new RedisTelemetryWriter(redis, NullLogger<RedisTelemetryWriter>.Instance);
        await using var publisher = new RabbitMqTelemetryPublisher(iotConfig, NullLogger<RabbitMqTelemetryPublisher>.Instance);
        await publisher.EnsureInfrastructureAsync(CancellationToken.None);

        for (var sensorId = 1; sensorId <= 20; sensorId++)
        {
            var sample = new TelemetrySample(sensorId, 40 + sensorId, "celsius", DateTimeOffset.UtcNow, "ok");
            Assert.True(await writer.WriteWithRetryAsync(sample, CancellationToken.None));
            Assert.True(await publisher.PublishWithRetryAsync(sample, CancellationToken.None));
        }

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (received.Count < 20 && DateTime.UtcNow < deadline)
            await Task.Delay(200);

        Assert.Equal(20, received.Count);
        Assert.Equal(Enumerable.Range(1, 20), received.Keys.OrderBy(x => x));
    }

    [Fact]
    public async Task Redis_stores_latest_value_for_each_sensor()
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(factory.RedisConnectionString);
        var writer = new RedisTelemetryWriter(redis, NullLogger<RedisTelemetryWriter>.Instance);

        for (var id = 1; id <= 20; id++)
        {
            var sample = new TelemetrySample(id, id * 1.5, "celsius", DateTimeOffset.UtcNow, "ok");
            await writer.WriteWithRetryAsync(sample, CancellationToken.None);
        }

        var db = redis.GetDatabase();
        for (var id = 1; id <= 20; id++)
        {
            var json = await db.StringGetAsync($"telemetry:sensor:{id}");
            Assert.True(json.HasValue);
            var doc = JsonDocument.Parse(json.ToString());
            Assert.Equal(id, doc.RootElement.GetProperty("sensorId").GetInt32());
        }
    }
}
