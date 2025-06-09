namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class Payment : AuditableEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? ExternalId { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, decimal amount, string? externalId = null)
    {
        Guard.Against.Default(orderId);
        Guard.Against.NegativeOrZero(amount);

        OrderId = orderId;
        Amount = amount;
        ExternalId = externalId;
        PaidAt = DateTime.UtcNow;
    }

    public void Confirm() => Status = PaymentStatus.Confirmed;
    public void Fail() => Status = PaymentStatus.Failed;
}

public enum PaymentStatus
{
    Pending,
    Confirmed,
    Failed
}

