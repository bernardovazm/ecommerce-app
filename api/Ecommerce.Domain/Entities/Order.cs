namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class Order : AuditableEntity<Guid>
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = default!;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items;

    public decimal Total => _items.Sum(i => i.Total);
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

    public void Confirm() => Status = OrderStatus.Confirmed;
    public void Cancel() => Status = OrderStatus.Canceled;
}
public enum OrderStatus
{
    Pending,
    Confirmed,
    Canceled,
    Shipped,
    Delivered
}
