namespace Ecommerce.Domain.Entities;

using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class Order : AuditableEntity<Guid>
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = default!;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items; public decimal Subtotal => _items.Sum(i => i.Total);
    public decimal ShippingCost { get; private set; } = 0;
    public decimal Total => Subtotal + ShippingCost;
    public string? ShippingAddress { get; private set; }
    public string? ShippingService { get; private set; }
    public int? ShippingDays { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    private Order() { }

    public Order(Guid customerId)
    {
        Guard.Against.Default(customerId);
        CustomerId = customerId;
    }

    public void AddItem(OrderItem item)
    {
        Guard.Against.Null(item);
        _items.Add(item);
    }

    public void SetShippingInfo(decimal shippingCost, string shippingAddress, string? shippingService = null, int? shippingDays = null)
    {
        Guard.Against.Negative(shippingCost);
        Guard.Against.NullOrWhiteSpace(shippingAddress);

        ShippingCost = shippingCost;
        ShippingAddress = shippingAddress;
        ShippingService = shippingService;
        ShippingDays = shippingDays;
    }

    public void Confirm() => Status = OrderStatus.Confirmed;
    public void Cancel() => Status = OrderStatus.Canceled;
    public void MarkPaymentPending() => Status = OrderStatus.PaymentPending;
    public void MarkPaymentProcessing() => Status = OrderStatus.PaymentProcessing;
    public void MarkPaymentFailed() => Status = OrderStatus.PaymentFailed;
}
public enum OrderStatus
{
    Pending,
    PaymentPending,
    PaymentProcessing,
    PaymentFailed,
    Confirmed,
    Canceled,
    Shipped,
    Delivered
}
