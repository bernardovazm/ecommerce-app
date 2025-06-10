namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class OrderItem : AuditableEntity<Guid>
{
    public Guid OrderId   { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity   { get; private set; }

    // Navigation properties
    public Product? Product { get; private set; }

    public decimal Total => UnitPrice * Quantity;

    private OrderItem() { }

    public OrderItem(Guid productId, decimal unitPrice, int quantity)
    {
        Guard.Against.Default(productId);
        Guard.Against.NegativeOrZero(unitPrice);
        Guard.Against.NegativeOrZero(quantity);

        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
