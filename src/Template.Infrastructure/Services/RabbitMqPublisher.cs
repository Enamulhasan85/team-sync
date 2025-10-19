using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Settings;

namespace Template.Infrastructure.Services;

/// <summary>
/// RabbitMQ implementation of IEventPublisher.
/// Publishes JSON-serialized messages to a Fanout exchange.
/// </summary>
public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
    {
        var settings = options.Value;
        _exchangeName = settings.ExchangeName;

        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Declare exchange (Fanout = broadcast)
            _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Fanout,
                durable: true
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to RabbitMQ: {ex.Message}", ex);
        }
    }

    public async Task PublishAsync<T>(string routingKey, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            mandatory: false,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}