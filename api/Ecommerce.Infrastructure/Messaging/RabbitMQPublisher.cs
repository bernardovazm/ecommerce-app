using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMQPublisher(
        IConfiguration configuration,
        ILogger<RabbitMQPublisher> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory()
        {
            HostName = configuration.GetConnectionString("RabbitMQ") ?? "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            DeclareQueues();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ on startup. Connection will be retried on first use.");
            _connection = null;
            _channel = null;
        }
    }

    private void DeclareQueues()
    {
        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel is not initialized.");
            return;
        }

        _channel.ExchangeDeclare("payment-dlx", ExchangeType.Direct);
        _channel.QueueDeclare("payment-failed", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("payment-failed", "payment-dlx", "payment-failed");

        var queueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "payment-dlx" },
            { "x-dead-letter-routing-key", "payment-failed" },
            { "x-message-ttl", 300000 }
        };

        _channel.QueueDeclare("payment-requests", durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
    }

    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel is not initialized. Cannot publish message to queue {QueueName}", queueName);
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");
            }

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

            _logger.LogInformation("Message published to queue {QueueName}: {MessageId}", queueName, properties.MessageId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }
    public async Task PublishPaymentRequestAsync(Guid paymentRequestId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentRequestRepository = scope.ServiceProvider.GetRequiredService<IPaymentRequestRepository>();

        var paymentRequest = await paymentRequestRepository.GetByIdAsync(paymentRequestId, cancellationToken);
        if (paymentRequest == null)
        {
            _logger.LogWarning("PaymentRequest not found: {PaymentRequestId}", paymentRequestId);
            return;
        }

        var message = new PaymentRequestMessage(
            paymentRequest.Id,
            paymentRequest.OrderId,
            paymentRequest.Amount,
            paymentRequest.PaymentMethod,
            paymentRequest.CustomerEmail,
            paymentRequest.RetryCount,
            paymentRequest.CreatedAt
        );

        await PublishAsync("payment-requests", message, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
