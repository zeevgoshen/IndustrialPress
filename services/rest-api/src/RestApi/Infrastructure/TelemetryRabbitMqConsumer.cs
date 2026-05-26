using System.Text;
using System.Text.Json;
using IndustrialPress.RestApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IndustrialPress.RestApi.Infrastructure;

public sealed class TelemetryRabbitMqConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelemetryRabbitMqConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public TelemetryRabbitMqConsumer(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<TelemetryRabbitMqConsumer> logger)
    {
        _services = services;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndConsumeAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "RabbitMQ consumer fault; reconnecting in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken cancellationToken)
    {
        var host = _configuration["RabbitMQ:Host"] ?? "localhost";
        var user = _configuration["RabbitMQ:User"] ?? "industrial";
        var password = _configuration["RabbitMQ:Password"] ?? "industrial";
        var exchange = _configuration["RabbitMQ:Exchange"] ?? "telemetry.events";
        var queue = _configuration["RabbitMQ:Queue"] ?? "telemetry.sensor-updates";
        var routingKey = _configuration["RabbitMQ:RoutingKey"] ?? "sensor.updated";

        var factory = new ConnectionFactory { HostName = host, UserName = user, Password = password };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(queue, exchange, routingKey, cancellationToken: cancellationToken);
        await _channel.BasicQosAsync(0, (ushort)_configuration.GetValue("RabbitMQ:Prefetch", 20), false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("sensorId", out var idProp))
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                    return;
                }

                var sensorId = idProp.GetInt32();
                using var scope = _services.CreateScope();
                var redis = scope.ServiceProvider.GetRequiredService<RedisTelemetryStore>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TelemetryHub>>();

                var payload = await redis.GetWithRetryAsync(sensorId, cancellationToken);
                if (payload is null)
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
                    return;
                }

                await hubContext.Clients.All.SendAsync("TelemetryUpdated", payload, cancellationToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                _logger.LogDebug("rmq_message_processed sensorId={SensorId}", sensorId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed processing RMQ message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(queue, autoAck: false, consumer, cancellationToken);
        _logger.LogInformation("RabbitMQ consumer listening on {Queue}", queue);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync(cancellationToken: cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
