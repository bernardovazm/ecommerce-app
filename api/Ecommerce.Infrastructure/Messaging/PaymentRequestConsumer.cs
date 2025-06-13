using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Services;

namespace Ecommerce.Infrastructure.Messaging;

public class PaymentRequestConsumer : BackgroundService, IMessageConsumer
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<PaymentRequestConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public PaymentRequestConsumer(
        IConfiguration configuration,
        ILogger<PaymentRequestConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMQAsync(stoppingToken);

        if (_connection != null && _channel != null)
        {
            await StartAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task InitializeRabbitMQAsync(CancellationToken stoppingToken)
    {
        int retryCount = 0;
        const int maxRetries = 10;
        const int retryDelayMs = 5000;

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ (attempt {Attempt}/{MaxRetries})", retryCount + 1, maxRetries);

                var factory = new ConnectionFactory()
                {
                    HostName = _configuration.GetConnectionString("RabbitMQ") ?? "localhost",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: "payment-requests", durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("Successfully connected to RabbitMQ");
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms...",
                    retryCount, maxRetries, retryDelayMs);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} attempts. RabbitMQ functionality will be disabled.", maxRetries);
                    return;
                }

                await Task.Delay(retryDelayMs, stoppingToken);
            }
        }
    }
    public new Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_channel == null)
        {
            _logger.LogWarning("Cannot start PaymentRequestConsumer: RabbitMQ channel is not available");
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var paymentRequest = JsonSerializer.Deserialize<PaymentRequestMessage>(message);
                if (paymentRequest != null)
                {
                    await ProcessPaymentRequestAsync(paymentRequest);
                    _channel?.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment request message: {Message}", message);

                _channel?.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(queue: "payment-requests", autoAck: false, consumer: consumer);
        _logger.LogInformation("Payment request consumer started");

        return Task.CompletedTask;
    }

    private async Task ProcessPaymentRequestAsync(PaymentRequestMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentRequestRepository = scope.ServiceProvider.GetRequiredService<IPaymentRequestRepository>();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        try
        {
            _logger.LogInformation("Processing payment request {PaymentRequestId} for order {OrderId}",
                message.PaymentRequestId, message.OrderId);

            var paymentRequest = await paymentRequestRepository.GetByIdAsync(message.PaymentRequestId);
            if (paymentRequest == null)
            {
                _logger.LogWarning("PaymentRequest not found: {PaymentRequestId}", message.PaymentRequestId);
                return;
            }

            var order = await orderRepository.GetByIdAsync(message.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
                paymentRequest.MarkAsCancelled();
                await paymentRequestRepository.UpdateAsync(paymentRequest);
                return;
            }

            paymentRequest.MarkAsProcessing();
            order.MarkPaymentProcessing();
            await paymentRequestRepository.UpdateAsync(paymentRequest);
            await orderRepository.UpdateAsync(order);

            var paymentResult = await paymentService.PayAsync(order); if (paymentResult.Success)
            {
                paymentRequest.MarkAsCompleted(paymentResult.GatewayId);
                order.Confirm();

                _logger.LogInformation("Payment successful for order {OrderId}, gateway ID: {GatewayId}",
                    message.OrderId, paymentResult.GatewayId);
                try
                {
                    var orderNotificationService = scope.ServiceProvider.GetRequiredService<IOrderNotificationService>();
                    await orderNotificationService.SendOrderConfirmationEmailAsync(order);
                    _logger.LogInformation("Order confirmation email sent for order {OrderId}", message.OrderId);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send order confirmation email for order {OrderId}", message.OrderId);
                }
            }
            else
            {
                paymentRequest.MarkAsFailed(paymentResult.Error ?? "Unknown payment error");
                order.MarkPaymentFailed();

                _logger.LogWarning("Payment failed for order {OrderId}: {Error}",
                    message.OrderId, paymentResult.Error);

                if (paymentRequest.CanRetry())
                {
                    var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
                    await Task.Delay(TimeSpan.FromMinutes(Math.Pow(2, paymentRequest.RetryCount)));
                    await messagePublisher.PublishPaymentRequestAsync(paymentRequest.Id);
                }
            }

            await paymentRequestRepository.UpdateAsync(paymentRequest);
            await orderRepository.UpdateAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment request {PaymentRequestId}", message.PaymentRequestId);
            throw;
        }
    }

    public new Task StopAsync(CancellationToken cancellationToken = default)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
