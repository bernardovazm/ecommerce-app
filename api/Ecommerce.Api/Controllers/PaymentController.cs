using Microsoft.AspNetCore.Mvc;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Orders.Commands;
using Ecommerce.Infrastructure.Payments.Pagarme;
using Ecommerce.Infrastructure.Payments.Pagarme.Models;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IPagarmeClient _pagarmeClient;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentRequestRepository paymentRequestRepository,
        IMessagePublisher messagePublisher,
        IPagarmeClient pagarmeClient,
        ILogger<PaymentController> logger)
    {
        _paymentRequestRepository = paymentRequestRepository;
        _messagePublisher = messagePublisher;
        _pagarmeClient = pagarmeClient;
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

            return Ok(new
            {
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

    [HttpPost("test-direct")]
    public async Task<IActionResult> TestDirectPayment([FromBody] TestDirectPaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Testing direct Pagar.me payment");

            var transactionRequest = new PagarmeTransactionRequest
            {
                Amount = (int)(request.Amount * 100),
                PaymentMethod = "credit_card",
                Capture = true,
                Async = false,
                SoftDescriptor = "ECOMMERCE TEST",
                Customer = new PagarmeCustomer
                {
                    ExternalId = Guid.NewGuid().ToString(),
                    Name = request.CustomerName,
                    Email = request.CustomerEmail,
                    Type = "individual",
                    Country = "br",
                    Documents = new List<PagarmeDocument>
                    {
                        new PagarmeDocument { Type = "cpf", Number = "11111111111" }
                    }
                },
                CardData = new PagarmeCardData
                {
                    Number = request.CardNumber,
                    HolderName = request.CardHolderName,
                    ExpirationDate = request.ExpirationDate,
                    Cvv = request.Cvv
                },
                Items = new List<PagarmeItem>
                {
                    new PagarmeItem
                    {
                        Id = "test-product",
                        Title = "Test Product",
                        UnitPrice = (int)(request.Amount * 100),
                        Quantity = 1,
                        Tangible = true
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "test", "true" },
                    { "scenario", request.Scenario }
                }
            };

            var response = await _pagarmeClient.CreateTransactionAsync(transactionRequest);

            return Ok(new
            {
                success = response.Status == "paid" || response.Status == "authorized",
                transactionId = response.Id,
                status = response.Status,
                amount = response.Amount / 100.0m,
                authorizationCode = response.AuthorizationCode,
                acquirerResponseCode = response.AcquirerResponseCode,
                refuseReason = response.RefuseReason,
                cardBrand = response.CardBrand,
                cardLastDigits = response.CardLastDigits,
                createdAt = response.DateCreated
            });
        }
        catch (PagarmeException ex)
        {
            _logger.LogError(ex, "Pagar.me error in direct payment test");
            return BadRequest(new { success = false, message = ex.Message, statusCode = ex.StatusCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in direct payment test");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("test-cards")]
    public IActionResult GetTestCards()
    {
        return Ok(new
        {
            approved = new
            {
                number = "4111111111111111",
                holderName = "João Silva",
                expirationDate = "1225",
                cvv = "123",
                description = "Card that will be approved"
            },
            declined = new
            {
                number = "4000000000000002",
                holderName = "Maria Santos",
                expirationDate = "1225",
                cvv = "123",
                description = "Card that will be declined"
            },
            processingError = new
            {
                number = "4000000000000119",
                holderName = "Pedro Costa",
                expirationDate = "1225",
                cvv = "123",
                description = "Card that will cause processing error"
            },
            insufficientFunds = new
            {
                number = "4000000000000341",
                holderName = "Ana Costa",
                expirationDate = "1225",
                cvv = "123",
                description = "Card with insufficient funds"
            }
        });
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<IActionResult> GetTransaction(string transactionId)
    {
        try
        {
            var transaction = await _pagarmeClient.GetTransactionAsync(transactionId);

            return Ok(new
            {
                id = transaction.Id,
                status = transaction.Status,
                amount = transaction.Amount / 100.0m,
                authorizationCode = transaction.AuthorizationCode,
                acquirerResponseCode = transaction.AcquirerResponseCode,
                refuseReason = transaction.RefuseReason,
                cardBrand = transaction.CardBrand,
                cardLastDigits = transaction.CardLastDigits,
                customer = new
                {
                    name = transaction.Customer.Name,
                    email = transaction.Customer.Email
                },
                createdAt = transaction.DateCreated,
                updatedAt = transaction.DateUpdated
            });
        }
        catch (PagarmeException ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", transactionId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class TestDirectPaymentRequest
{
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string Scenario { get; set; } = "test";
}

public class TestPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "credit_card";
    public string CustomerEmail { get; set; } = string.Empty;
}
