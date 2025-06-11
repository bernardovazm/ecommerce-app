namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class PaymentRequest : AuditableEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public string PaymentMethod { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public PaymentRequestStatus Status { get; private set; } = PaymentRequestStatus.Pending;
    public int RetryCount { get; private set; } = 0;
    public string? ErrorMessage { get; private set; }
    public string? ExternalPaymentId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private PaymentRequest() { }

    public PaymentRequest(Guid orderId, decimal amount, string paymentMethod, string customerEmail)
    {
        Guard.Against.Default(orderId);
        Guard.Against.NegativeOrZero(amount);
        Guard.Against.NullOrWhiteSpace(paymentMethod);
        Guard.Against.NullOrWhiteSpace(customerEmail);

        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        CustomerEmail = customerEmail;
    }

    public void MarkAsProcessing()
    {
        Status = PaymentRequestStatus.Processing;
    }

    public void MarkAsCompleted(string? externalPaymentId = null)
    {
        Status = PaymentRequestStatus.Completed;
        ExternalPaymentId = externalPaymentId;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Guard.Against.NullOrWhiteSpace(errorMessage);
        Status = PaymentRequestStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    public void MarkAsCancelled()
    {
        Status = PaymentRequestStatus.Cancelled;
    }

    public bool CanRetry() => RetryCount < 3 && Status == PaymentRequestStatus.Failed;
}

public enum PaymentRequestStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
