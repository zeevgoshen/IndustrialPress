using System.Text;
using System.Text.Json;
using IndustrialPress.IotTelemetry.Models;
using RabbitMQ.Client;

namespace IndustrialPress.IotTelemetry.Infrastructure;

public sealed class RabbitMqTelemetryPublisher : IAsyncDisposable
{
    private readonly ILogger<RabbitMqTelemetryPublisher> _logger;
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly string _exchange;
    private readonly string _routingKey;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqTelemetryPublisher(IConfiguration configuration, ILogger<RabbitMqTelemetryPublisher> logger)
    {
        _logger = logger;
        _host = configuration["RabbitMQ:Host"] ?? "localhost";
        _port = configuration.GetValue("RabbitMQ:Port", 5672);
        _user = configuration["RabbitMQ:User"] ?? "industrial";
        _password = configuration["RabbitMQ:Password"] ?? "industrial";
        _exchange = configuration["RabbitMQ:Exchange"] ?? "telemetry.events";
        _routingKey = configuration["RabbitMQ:RoutingKey"] ?? "sensor.updated";
    }

    public async Task EnsureInfrastructureAsync(CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);
        if (_channel is null) return;
        await _channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> PublishWithRetryAsync(TelemetrySample sample, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new
        {
            sensorId = sample.SensorId,
            updatedAt = sample.Timestamp.UtcDateTime
        });

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await RetryDelays.DelayBeforeAttempt(attempt, RetryDelays.IoTPublishBackoffMs, cancellationToken);
            try
            {
                await EnsureConnectedAsync(cancellationToken);
                if (_channel is null) continue;

                var props = new BasicProperties { Persistent = true };
                await _channel.BasicPublishAsync(
                    exchange: _exchange,
                    routingKey: _routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: body,
                    cancellationToken: cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "rmq_publish_failed sensorId={SensorId} attempt={Attempt}", sample.SensorId, attempt + 1);
                await ResetConnectionAsync();
            }
        }

        _logger.LogError("rmq_publish_failed sensorId={SensorId} exhausted retries", sample.SensorId);
        return false;
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true }) return;

        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _user,
            Password = _password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    private async Task ResetConnectionAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync() => await ResetConnectionAsync();
}
