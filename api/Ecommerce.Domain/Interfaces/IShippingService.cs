using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Interfaces;

public interface IShippingService
{
    Task<ShippingCalculationResponse> CalculateShippingAsync(ShippingCalculationRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsServiceAvailable(string zipCode, CancellationToken cancellationToken = default);
    string GetProviderName();
}

public interface IShippingCalculationService
{
    Task<ShippingCalculationResponse> CalculateShippingForOrderAsync(
        List<Guid> productIds,
        List<int> quantities,
        string destinationZipCode,
        CancellationToken cancellationToken = default);

    Task<ShippingCalculationResponse> GetShippingOptionsAsync(
        string destinationZipCode,
        decimal weight,
        decimal length,
        decimal width,
        decimal height,
        decimal declaredValue,
        CancellationToken cancellationToken = default);
}
