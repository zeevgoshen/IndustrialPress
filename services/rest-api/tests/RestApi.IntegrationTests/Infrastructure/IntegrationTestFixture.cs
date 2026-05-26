using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace IndustrialPress.RestApi.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime, IAsyncDisposable
{
    private readonly RedisContainer _redis = new RedisBuilder().Build();
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithUsername("industrial")
        .WithPassword("industrial")
        .Build();

    public string RedisConnectionString { get; private set; } = "";
    public string RabbitMqHost { get; private set; } = "localhost";
    public int RabbitMqPort { get; private set; }

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
        await _rabbit.StartAsync();
        RedisConnectionString = _redis.GetConnectionString();
        RabbitMqHost = _rabbit.Hostname;
        RabbitMqPort = _rabbit.GetMappedPublicPort(5672);
    }

    public async ValueTask DisposeAsync()
    {
        await _redis.DisposeAsync();
        await _rabbit.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = RedisConnectionString,
                ["RabbitMQ:Host"] = RabbitMqHost,
                ["RabbitMQ:Port"] = RabbitMqPort.ToString(),
                ["RabbitMQ:User"] = "industrial",
                ["RabbitMQ:Password"] = "industrial",
                ["Grpc:SqlData"] = "http://localhost:1"
            });
        });
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
