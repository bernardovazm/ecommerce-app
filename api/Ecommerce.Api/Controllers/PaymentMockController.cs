using Microsoft.AspNetCore.Mvc;
using Ecommerce.Infrastructure.Payments.Pagarme.Models;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMockController : ControllerBase
{
    private readonly ILogger<PaymentMockController> _logger;

    public PaymentMockController(ILogger<PaymentMockController> logger)
    {
        _logger = logger;
    }

    [HttpPost("simulate-pagarme")]
    public IActionResult SimulatePagarmePayment([FromBody] SimulatePaymentRequest request)
    {
        _logger.LogInformation("Simulating Pagar.me payment for amount: {Amount}", request.Amount);

        // Simulate different scenarios based on card number
        var response = request.CardNumber switch
        {
            "4111111111111111" => SimulateApprovedPayment(request),
            "4000000000000002" => SimulateDeclinedPayment(request),
            "4000000000000119" => SimulateProcessingError(request),
            "4000000000000341" => SimulateInsufficientFunds(request),
            _ => SimulateUnknownCard(request)
        };

        return Ok(response);
    }

    [HttpGet("test-scenarios")]
    public IActionResult GetTestScenarios()
    {
        return Ok(new
        {
            scenarios = new[]
            {
                new
                {
                    name = "Pagamento Aprovado",
                    cardNumber = "4111111111111111",
                    expectedStatus = "paid",
                    description = "Transação aprovada com sucesso"
                },
                new
                {
                    name = "Pagamento Recusado",
                    cardNumber = "4000000000000002",
                    expectedStatus = "refused",
                    description = "Transação recusada pelo banco emissor"
                },
                new
                {
                    name = "Erro de Processamento",
                    cardNumber = "4000000000000119",
                    expectedStatus = "processing",
                    description = "Erro durante o processamento"
                },
                new
                {
                    name = "Saldo Insuficiente",
                    cardNumber = "4000000000000341",
                    expectedStatus = "refused",
                    description = "Saldo insuficiente na conta"
                }
            },
            commonData = new
            {
                holderName = "Test Customer",
                expirationDate = "1225",
                cvv = "123"
            }
        });
    }

    [HttpPost("simulate-webhook")]
    public IActionResult SimulateWebhook([FromBody] WebhookSimulationRequest request)
    {
        _logger.LogInformation("Simulating Pagar.me webhook for transaction: {TransactionId}", request.TransactionId);

        var webhookPayload = new
        {
            id = request.TransactionId,
            fingerprint = Guid.NewGuid().ToString(),
            @event = "transaction_status_changed",
            old_status = request.OldStatus,
            desired_status = request.NewStatus,
            @object = "transaction",
            transaction = new
            {
                id = request.TransactionId,
                status = request.NewStatus,
                amount = request.Amount * 100, // Convert to cents
                paid_amount = request.NewStatus == "paid" ? request.Amount * 100 : 0,
                authorization_code = request.NewStatus == "paid" ? "123456" : null,
                tid = Random.Shared.Next(100000, 999999),
                nsu = Random.Shared.Next(100000, 999999),
                date_created = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                date_updated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            }
        };

        return Ok(new
        {
            message = "Webhook simulation created",
            payload = webhookPayload,
            instructions = "Use this payload to test webhook processing in your application"
        });
    }

    [HttpGet("integration-status")]
    public IActionResult GetIntegrationStatus()
    {
        return Ok(new
        {
            status = "mock_mode",
            message = "Pagar.me integration is running in mock mode",
            features = new
            {
                directPayments = true,
                asyncPayments = true,
                webhooks = true,
                testCards = true,
                realApiIntegration = false
            },
            testEndpoints = new
            {
                simulatePayment = "/api/paymentmock/simulate-pagarme",
                testScenarios = "/api/paymentmock/test-scenarios",
                simulateWebhook = "/api/paymentmock/simulate-webhook",
                integrationStatus = "/api/paymentmock/integration-status"
            }
        });
    }

    private object SimulateApprovedPayment(SimulatePaymentRequest request)
    {
        return new
        {
            success = true,
            transactionId = Random.Shared.Next(10000, 99999),
            status = "paid",
            amount = request.Amount,
            authorizationCode = "123456",
            acquirerResponseCode = "0000",
            refuseReason = (string?)null,
            cardBrand = GetCardBrand(request.CardNumber),
            cardLastDigits = request.CardNumber.Substring(request.CardNumber.Length - 4),
            createdAt = DateTime.UtcNow,
            processingTime = TimeSpan.FromSeconds(2.5),
            simulation = new
            {
                scenario = "approved",
                mockResponse = true,
                realPagarmeIntegration = false
            }
        };
    }

    private object SimulateDeclinedPayment(SimulatePaymentRequest request)
    {
        return new
        {
            success = false,
            transactionId = Random.Shared.Next(10000, 99999),
            status = "refused",
            amount = request.Amount,
            authorizationCode = (string?)null,
            acquirerResponseCode = "1001",
            refuseReason = "card_refused",
            cardBrand = GetCardBrand(request.CardNumber),
            cardLastDigits = request.CardNumber.Substring(request.CardNumber.Length - 4),
            createdAt = DateTime.UtcNow,
            processingTime = TimeSpan.FromSeconds(1.8),
            simulation = new
            {
                scenario = "declined",
                mockResponse = true,
                realPagarmeIntegration = false
            }
        };
    }

    private object SimulateProcessingError(SimulatePaymentRequest request)
    {
        return new
        {
            success = false,
            transactionId = Random.Shared.Next(10000, 99999),
            status = "processing",
            amount = request.Amount,
            authorizationCode = (string?)null,
            acquirerResponseCode = "9999",
            refuseReason = "processing_error",
            cardBrand = GetCardBrand(request.CardNumber),
            cardLastDigits = request.CardNumber.Substring(request.CardNumber.Length - 4),
            createdAt = DateTime.UtcNow,
            processingTime = TimeSpan.FromSeconds(5.2),
            simulation = new
            {
                scenario = "processing_error",
                mockResponse = true,
                realPagarmeIntegration = false
            }
        };
    }

    private object SimulateInsufficientFunds(SimulatePaymentRequest request)
    {
        return new
        {
            success = false,
            transactionId = Random.Shared.Next(10000, 99999),
            status = "refused",
            amount = request.Amount,
            authorizationCode = (string?)null,
            acquirerResponseCode = "1002",
            refuseReason = "insufficient_funds",
            cardBrand = GetCardBrand(request.CardNumber),
            cardLastDigits = request.CardNumber.Substring(request.CardNumber.Length - 4),
            createdAt = DateTime.UtcNow,
            processingTime = TimeSpan.FromSeconds(1.5),
            simulation = new
            {
                scenario = "insufficient_funds",
                mockResponse = true,
                realPagarmeIntegration = false
            }
        };
    }

    private object SimulateUnknownCard(SimulatePaymentRequest request)
    {
        return new
        {
            success = false,
            transactionId = (int?)null,
            status = "error",
            amount = request.Amount,
            authorizationCode = (string?)null,
            acquirerResponseCode = "9998",
            refuseReason = "invalid_card",
            cardBrand = "unknown",
            cardLastDigits = request.CardNumber.Length >= 4 ? request.CardNumber.Substring(request.CardNumber.Length - 4) : "****",
            createdAt = DateTime.UtcNow,
            processingTime = TimeSpan.FromSeconds(0.5),
            simulation = new
            {
                scenario = "unknown_card",
                mockResponse = true,
                realPagarmeIntegration = false
            }
        };
    }

    private string GetCardBrand(string cardNumber)
    {
        return cardNumber.StartsWith("4") ? "visa" :
               cardNumber.StartsWith("5") ? "mastercard" :
               cardNumber.StartsWith("3") ? "amex" : "unknown";
    }
}

public class SimulatePaymentRequest
{
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}

public class WebhookSimulationRequest
{
    public int TransactionId { get; set; }
    public string OldStatus { get; set; } = "processing";
    public string NewStatus { get; set; } = "paid";
    public decimal Amount { get; set; }
}
