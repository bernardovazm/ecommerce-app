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
    private const string DefaultUserName = "guest";
    private const string DefaultPassword = "guest";
    private const string PaymentDlxExchange = "payment-dlx";
    private const string PaymentFailedQueue = "payment-failed";
    private const string PaymentRequestsQueue = "payment-requests";
    private const string OrderCreatedQueue = "order-created";

    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration; public RabbitMQPublisher(
        IConfiguration configuration,
        ILogger<RabbitMQPublisher> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration; var factory = new ConnectionFactory()
        {
            HostName = _configuration.GetConnectionString("RabbitMQ") ?? "localhost",
            Port = 5672,
            UserName = DefaultUserName,
            Password = DefaultPassword,
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

        try
        {
            _channel.ExchangeDeclare(PaymentDlxExchange, ExchangeType.Direct, durable: true);

            _channel.QueueDeclare(PaymentFailedQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(PaymentFailedQueue, PaymentDlxExchange, PaymentFailedQueue);
            _channel.QueueDeclare(PaymentRequestsQueue, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueDeclare(OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false);

            _logger.LogInformation("Payment requests queue declared successfully");
            _logger.LogInformation("Order created notification queue declared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare RabbitMQ queues and exchanges");
            throw;
        }
    }
    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel is not initialized. Cannot publish message to queue {QueueName}", queueName);
                if (!TryReconnect())
                {
                    throw new InvalidOperationException("RabbitMQ channel is not initialized and reconnection failed.");
                }
            }

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

            _logger.LogInformation("Message published to queue {QueueName}: {MessageId}", queueName, properties.MessageId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}: {Message}", queueName, ex.Message);
            throw;
        }
    }
    private bool TryReconnect()
    {
        try
        {
            _logger.LogInformation("Attempting to reconnect to RabbitMQ...");

            var factory = new ConnectionFactory()
            {
                HostName = _configuration.GetConnectionString("RabbitMQ") ?? "localhost",
                Port = 5672,
                UserName = DefaultUserName,
                Password = DefaultPassword,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            DeclareQueuesWithChannel(channel);

            _channel?.Close();
            _connection?.Close();

            _connection = connection;
            _channel = channel;

            _logger.LogInformation("RabbitMQ reconnection successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reconnect to RabbitMQ: {Message}", ex.Message);
            return false;
        }
    }
    private static void DeclareQueuesWithChannel(IModel channel)
    {
        try
        {
            channel.ExchangeDeclare(PaymentDlxExchange, ExchangeType.Direct, durable: true);

            channel.QueueDeclare(PaymentFailedQueue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(PaymentFailedQueue, PaymentDlxExchange, PaymentFailedQueue);
            channel.QueueDeclare(PaymentRequestsQueue, durable: true, exclusive: false, autoDelete: false);

            channel.QueueDeclare(OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false);
        }
        catch (Exception)
        {
            channel.QueueDeclare(PaymentRequestsQueue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueDeclare(OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false);
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

        await PublishAsync(PaymentRequestsQueue, message, cancellationToken);
    }
    public async Task PublishOrderCreatedAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<Domain.Repositories.IOrderRepository>();

        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", orderId);
            return;
        }

        var message = new OrderCreatedMessage(
            order.Id,
            order.CustomerId,
            order.Total,
            order.Items.Count,
            order.Customer?.Email ?? "unknown@email.com",
            order.ShippingAddress ?? "No shipping address",
            order.CreatedAt
        );

        await PublishAsync(OrderCreatedQueue, message, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
