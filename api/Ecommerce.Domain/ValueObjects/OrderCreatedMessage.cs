namespace Ecommerce.Domain.ValueObjects;

public record OrderCreatedMessage(
    Guid OrderId,
    Guid CustomerId,
    decimal Total,
    int ItemCount,
    string CustomerEmail,
    string ShippingAddress,
    DateTime CreatedAt
)
{
    public OrderCreatedMessage() : this(Guid.Empty, Guid.Empty, 0, 0, string.Empty, string.Empty, DateTime.UtcNow)
    {
    }
}
