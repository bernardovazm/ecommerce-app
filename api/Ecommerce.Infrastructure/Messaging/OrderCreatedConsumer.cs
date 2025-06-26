using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Infrastructure.Messaging;

public class OrderCreatedConsumer : BackgroundService
{
    private const string DefaultUserName = "guest";
    private const string DefaultPassword = "guest";
    private const string OrderCreatedQueue = "order-created";

    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(
        ILogger<OrderCreatedConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var retryCount = 0;
        const int maxRetries = 10;

        while (!stoppingToken.IsCancellationRequested && retryCount < maxRetries)
        {
            try
            {
                retryCount++;
                _logger.LogInformation("Attempting to connect to RabbitMQ for order notifications (attempt {Attempt}/{MaxRetries})", retryCount, maxRetries);

                var factory = new ConnectionFactory()
                {
                    HostName = _configuration.GetConnectionString("RabbitMQ") ?? "localhost",
                    Port = 5672,
                    UserName = DefaultUserName,
                    Password = DefaultPassword,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false);
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var jsonMessage = Encoding.UTF8.GetString(body);

                        var orderCreatedMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(jsonMessage);

                        if (orderCreatedMessage != null)
                        {
                            _logger.LogInformation("Order created notification received: OrderId={OrderId}, CustomerId={CustomerId}, Total={Total}, Email={Email}",
                                orderCreatedMessage.OrderId,
                                orderCreatedMessage.CustomerId,
                                orderCreatedMessage.Total,
                                orderCreatedMessage.CustomerEmail);

                            await Task.Delay(100, stoppingToken);

                            _logger.LogInformation("Order notification processed successfully for OrderId={OrderId}", orderCreatedMessage.OrderId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize order created message: {Message}", jsonMessage);
                        }

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing order created notification");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _channel.BasicConsume(queue: OrderCreatedQueue, autoAck: false, consumer: consumer);

                _logger.LogInformation("Order created consumer started successfully");
                retryCount = 0;

                while (!stoppingToken.IsCancellationRequested && _connection.IsOpen)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start order created consumer (attempt {Attempt}/{MaxRetries}): {Message}", retryCount, maxRetries, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5 * retryCount), stoppingToken);
            }
        }

        if (retryCount >= maxRetries)
        {
            _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} attempts. Order created consumer will not start.", maxRetries);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
