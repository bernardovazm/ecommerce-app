using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Infrastructure.Payments.Pagarme;
using Ecommerce.Infrastructure.Payments.Pagarme.Configuration;
using Ecommerce.Infrastructure.Payments.Pagarme.Models;

namespace Ecommerce.Infrastructure.Payments;

public class PagarmePaymentService : IPaymentService
{
    private readonly IPagarmeClient _pagarmeClient;
    private readonly PagarmeSettings _settings;
    private readonly ILogger<PagarmePaymentService> _logger;

    public PagarmePaymentService(
        IPagarmeClient pagarmeClient,
        IOptions<PagarmeSettings> settings,
        ILogger<PagarmePaymentService> logger)
    {
        _pagarmeClient = pagarmeClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentResult> PayAsync(Order order, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId}, amount: {Amount}", order.Id, order.Total);

            var transactionRequest = CreateTransactionRequest(order);
            var response = await _pagarmeClient.CreateTransactionAsync(transactionRequest, ct);

            return ProcessPagarmeResponse(response);
        }
        catch (PagarmeException ex)
        {
            _logger.LogError(ex, "Pagar.me error processing payment for order {OrderId}", order.Id);
            return new PaymentResult(false, null, $"Payment gateway error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", order.Id);
            return new PaymentResult(false, null, $"Payment processing error: {ex.Message}");
        }
    }

    private PagarmeTransactionRequest CreateTransactionRequest(Order order)
    {
        // Convert amount to cents (Pagar.me uses cents)
        var amountInCents = (int)(order.Total * 100);

        var request = new PagarmeTransactionRequest
        {
            Amount = amountInCents,
            PaymentMethod = "credit_card",
            Capture = true,
            Async = false,
            SoftDescriptor = "ECOMMERCE",
            Customer = CreateCustomer(order),
            Items = CreateItems(order),
            CardData = GetTestCardData(), // Using test card for sandbox
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() },
                { "customer_id", order.CustomerId.ToString() },
                { "environment", _settings.IsSandbox ? "sandbox" : "production" }
            }
        };

        return request;
    }

    private PagarmeCustomer CreateCustomer(Order order)
    {
        return new PagarmeCustomer
        {
            ExternalId = order.CustomerId.ToString(),
            Name = order.Customer?.FirstName + " " + order.Customer?.LastName ?? "Test Customer",
            Email = order.Customer?.Email ?? "test@example.com",
            Type = "individual",
            Country = "br",
            Documents = new List<PagarmeDocument>
            {
                new PagarmeDocument
                {
                    Type = "cpf",
                    Number = "11111111111" // Test CPF
                }
            },
            Phones = new List<PagarmePhone>
            {
                new PagarmePhone
                {
                    Country = "55",
                    Area = "11",
                    Number = "999999999",
                    Type = "mobile"
                }
            }
        };
    }

    private List<PagarmeItem> CreateItems(Order order)
    {
        return order.Items.Select(item => new PagarmeItem
        {
            Id = item.ProductId.ToString(),
            Title = item.Product?.Name ?? "Product",
            UnitPrice = (int)(item.UnitPrice * 100), // Convert to cents
            Quantity = item.Quantity,
            Tangible = true
        }).ToList();
    }    private PagarmeCardData GetTestCardData()
    {
        // For sandbox environment, we'll use test cards
        // In production, this would come from the payment form
        if (_settings.IsSandbox)
        {
            // Simulate different scenarios based on random selection
            var random = new Random();
            var scenario = random.Next(1, 4);
            
            var testCard = scenario switch
            {
                1 => _settings.TestCards.Approved,  // 33% success
                2 => _settings.TestCards.Declined,  // 33% declined
                _ => _settings.TestCards.ProcessingError  // 33% processing error
            };

            return new PagarmeCardData
            {
                Number = testCard.Number,
                HolderName = testCard.HolderName,
                ExpirationDate = testCard.ExpirationDate,
                Cvv = testCard.Cvv
            };
        }

        // In production, this should come from encrypted card data from frontend
        throw new InvalidOperationException("Card data must be provided for production environment");
    }

    private PaymentResult ProcessPagarmeResponse(PagarmeTransactionResponse response)
    {
        var isSuccess = response.Status == "paid" || response.Status == "authorized";
        var gatewayId = response.Id.ToString();
        string? errorMessage = null;

        if (!isSuccess)
        {
            errorMessage = response.Status switch
            {
                "refused" => $"Payment refused: {response.RefuseReason}",
                "processing" => "Payment is still processing",
                "pending_capture" => "Payment authorized but pending capture",
                "chargedback" => "Payment was charged back",
                "refunded" => "Payment was refunded",
                _ => $"Payment failed with status: {response.Status}"
            };

            _logger.LogWarning("Payment failed for transaction {TransactionId}: {Status} - {Reason}", 
                response.Id, response.Status, response.RefuseReason);
        }
        else
        {
            _logger.LogInformation("Payment successful for transaction {TransactionId}: {Status}", 
                response.Id, response.Status);
        }

        return new PaymentResult(isSuccess, gatewayId, errorMessage);
    }
}
