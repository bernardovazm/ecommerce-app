namespace Ecommerce.Domain.Entities;

using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public sealed class ShippingQuote : AuditableEntity<Guid>
{
    public string OriginZipCode { get; private set; } = default!;
    public string DestinationZipCode { get; private set; } = default!;
    public decimal Weight { get; private set; }
    public decimal Length { get; private set; }
    public decimal Width { get; private set; }
    public decimal Height { get; private set; }
    public string ServiceCode { get; private set; } = default!;
    public string ServiceName { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int DeliveryDays { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public string? Observations { get; private set; }

    private ShippingQuote() { }

    public ShippingQuote(
        string originZipCode,
        string destinationZipCode,
        decimal weight,
        decimal length,
        decimal width,
        decimal height,
        string serviceCode,
        string serviceName,
        decimal price,
        int deliveryDays,
        DateTime validUntil,
        string? observations = null)
    {
        Guard.Against.NullOrWhiteSpace(originZipCode);
        Guard.Against.NullOrWhiteSpace(destinationZipCode);
        Guard.Against.NegativeOrZero(weight);
        Guard.Against.NegativeOrZero(length);
        Guard.Against.NegativeOrZero(width);
        Guard.Against.NegativeOrZero(height);
        Guard.Against.NullOrWhiteSpace(serviceCode);
        Guard.Against.NullOrWhiteSpace(serviceName);
        Guard.Against.Negative(price);
        Guard.Against.Negative(deliveryDays);

        OriginZipCode = originZipCode;
        DestinationZipCode = destinationZipCode;
        Weight = weight;
        Length = length;
        Width = width;
        Height = height;
        ServiceCode = serviceCode;
        ServiceName = serviceName;
        Price = price;
        DeliveryDays = deliveryDays;
        ValidUntil = validUntil;
        Observations = observations;
    }

    public bool IsValid() => DateTime.UtcNow <= ValidUntil;
}
