using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Orders.Commands;

public class ProcessOrderPaymentCommand
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

public class ProcessOrderPaymentHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessOrderPaymentHandler> _logger;

    public ProcessOrderPaymentHandler(
        IOrderRepository orderRepository,
        IPaymentRequestRepository paymentRequestRepository,
        IMessagePublisher messagePublisher,
        IPaymentService paymentService,
        ILogger<ProcessOrderPaymentHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentRequestRepository = paymentRequestRepository;
        _messagePublisher = messagePublisher;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<ProcessOrderPaymentResult> HandleAsync(ProcessOrderPaymentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
            {
                return ProcessOrderPaymentResult.Failure("Order not found");
            }

            _logger.LogInformation("Processing payment for order {OrderId}", command.OrderId);

            // First, try direct payment
            try
            {
                var directPaymentResult = await _paymentService.PayAsync(order, cancellationToken);
                
                if (directPaymentResult.Success)
                {
                    order.Confirm();
                    await _orderRepository.UpdateAsync(order, cancellationToken);
                    
                    _logger.LogInformation("Direct payment successful for order {OrderId}", command.OrderId);
                    return ProcessOrderPaymentResult.Success("Payment processed successfully", directPaymentResult.GatewayId);
                }
                
                _logger.LogWarning("Direct payment failed for order {OrderId}: {Error}", command.OrderId, directPaymentResult.Error);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Direct payment failed for order {OrderId}, falling back to async processing", command.OrderId);
            }

            // If direct payment fails, create async payment request
            var paymentRequest = new PaymentRequest(
                command.OrderId,
                order.Total,
                command.PaymentMethod,
                command.CustomerEmail
            );

            await _paymentRequestRepository.CreateAsync(paymentRequest, cancellationToken);

            // Update order status
            order.MarkPaymentPending();
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Publish to message queue for async processing
            await _messagePublisher.PublishPaymentRequestAsync(paymentRequest.Id, cancellationToken);

            _logger.LogInformation("Payment request queued for order {OrderId}, PaymentRequest {PaymentRequestId}", 
                command.OrderId, paymentRequest.Id);

            return ProcessOrderPaymentResult.Pending("Payment request has been queued for processing", paymentRequest.Id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", command.OrderId);
            return ProcessOrderPaymentResult.Failure($"Internal error: {ex.Message}");
        }
    }
}

public class ProcessOrderPaymentResult
{
    public bool IsSuccess { get; private set; }
    public bool IsPending { get; private set; }
    public string Message { get; private set; }
    public string? Reference { get; private set; }

    private ProcessOrderPaymentResult(bool isSuccess, bool isPending, string message, string? reference = null)
    {
        IsSuccess = isSuccess;
        IsPending = isPending;
        Message = message;
        Reference = reference;
    }

    public static ProcessOrderPaymentResult Success(string message, string? reference = null) 
        => new(true, false, message, reference);
    
    public static ProcessOrderPaymentResult Pending(string message, string? reference = null) 
        => new(false, true, message, reference);
    
    public static ProcessOrderPaymentResult Failure(string message) 
        => new(false, false, message);
}
