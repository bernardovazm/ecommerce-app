namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class Shipment : AuditableEntity<Guid>
{
    public Guid OrderId   { get; private set; }
    public string Address { get; private set; } = default!;
    public DateTime ShippedAt { get; private set; }
    public string TrackingCode { get; private set; } = default!;
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Preparing;

    private Shipment() { }

    public Shipment(Guid orderId, string address, string trackingCode)
    {
        Guard.Against.Default(orderId);
        Guard.Against.NullOrWhiteSpace(address);
        Guard.Against.NullOrWhiteSpace(trackingCode);

        OrderId = orderId;
        Address = address;
        TrackingCode = trackingCode;
        ShippedAt = DateTime.UtcNow;
    }

    public void MarkDelivered() => Status = ShipmentStatus.Delivered;
}
public enum ShipmentStatus
{
    Preparing,
    Shipped,
    Delivered
}
