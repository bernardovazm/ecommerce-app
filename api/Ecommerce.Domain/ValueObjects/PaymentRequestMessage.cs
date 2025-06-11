namespace Ecommerce.Domain.ValueObjects;

public record PaymentRequestMessage(
    Guid PaymentRequestId,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string CustomerEmail,
    int RetryCount = 0,
    DateTime RequestedAt = default
)
{
    public PaymentRequestMessage() : this(Guid.Empty, Guid.Empty, 0, string.Empty, string.Empty)
    {
        RequestedAt = DateTime.UtcNow;
    }
}
