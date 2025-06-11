using Microsoft.AspNetCore.Mvc;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Orders.Commands;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentRequestRepository paymentRequestRepository,
        IMessagePublisher messagePublisher,
        ILogger<PaymentController> logger)
    {
        _paymentRequestRepository = paymentRequestRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    [HttpPost("test-async")]
    public async Task<IActionResult> TestAsyncPayment([FromBody] TestPaymentRequest request)
    {
        try
        {
            var paymentRequest = new PaymentRequest(
                request.OrderId,
                request.Amount,
                request.PaymentMethod,
                request.CustomerEmail
            );

            await _paymentRequestRepository.CreateAsync(paymentRequest);

            await _messagePublisher.PublishPaymentRequestAsync(paymentRequest.Id);

            _logger.LogInformation("Test payment request created and queued: {PaymentRequestId}", paymentRequest.Id);

            return Ok(new { 
                success = true, 
                paymentRequestId = paymentRequest.Id,
                message = "Payment request has been queued for processing" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test payment request");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetPaymentRequest(Guid id)
    {
        try
        {
            var paymentRequest = await _paymentRequestRepository.GetByIdAsync(id);
            if (paymentRequest == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                id = paymentRequest.Id,
                orderId = paymentRequest.OrderId,
                amount = paymentRequest.Amount,
                paymentMethod = paymentRequest.PaymentMethod,
                customerEmail = paymentRequest.CustomerEmail,
                status = paymentRequest.Status.ToString(),
                retryCount = paymentRequest.RetryCount,
                errorMessage = paymentRequest.ErrorMessage,
                externalPaymentId = paymentRequest.ExternalPaymentId,
                createdAt = paymentRequest.CreatedAt,
                processedAt = paymentRequest.ProcessedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment request {PaymentRequestId}", id);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingPaymentRequests()
    {
        try
        {
            var pendingRequests = await _paymentRequestRepository.GetPendingRequestsAsync();
            return Ok(pendingRequests.Select(pr => new
            {
                id = pr.Id,
                orderId = pr.OrderId,
                amount = pr.Amount,
                paymentMethod = pr.PaymentMethod,
                customerEmail = pr.CustomerEmail,
                status = pr.Status.ToString(),
                retryCount = pr.RetryCount,
                createdAt = pr.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payment requests");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class TestPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "credit_card";
    public string CustomerEmail { get; set; } = string.Empty;
}
