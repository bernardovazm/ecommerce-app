namespace Ecommerce.Domain.Models;

public class ShippingCalculationRequest
{
    public string OriginZipCode { get; set; } = default!;
    public string DestinationZipCode { get; set; } = default!;
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal DeclaredValue { get; set; }
    public List<string>? ServiceCodes { get; set; }
}

public class ShippingCalculationResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ShippingOption> Options { get; set; } = new();
}

public class ShippingOption
{
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public decimal Price { get; set; }
    public int DeliveryDays { get; set; }
    public string? Observations { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class ProductDimensions
{
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
}

public class ShippingAddress
{
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string Country { get; set; } = default!;
}
