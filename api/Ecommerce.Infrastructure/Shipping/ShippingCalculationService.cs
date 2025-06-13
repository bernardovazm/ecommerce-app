using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;
using Ecommerce.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Shipping;

public class ShippingCalculationService : IShippingCalculationService
{
    private readonly IShippingService _shippingService;
    private readonly IProductReadRepository _productRepository;
    private readonly ILogger<ShippingCalculationService> _logger;
    private readonly string _defaultOriginZipCode;

    public ShippingCalculationService(
        IShippingService shippingService,
        IProductReadRepository productRepository,
        IConfiguration configuration,
        ILogger<ShippingCalculationService> logger)
    {
        _shippingService = shippingService;
        _productRepository = productRepository;
        _logger = logger;
        _defaultOriginZipCode = configuration.GetValue<string>("Correios:DefaultOriginZipCode") ?? "01310-100";
    }

    public async Task<ShippingCalculationResponse> CalculateShippingForOrderAsync(
        List<Guid> productIds,
        List<int> quantities,
        string destinationZipCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productIds.Count != quantities.Count)
            {
                return new ShippingCalculationResponse
                {
                    Success = false,
                    ErrorMessage = "Product IDs and quantities lists must have the same length"
                };
            }

            _logger.LogInformation("Calculating shipping for {ProductCount} products to {Destination}",
                productIds.Count, destinationZipCode);
            var products = new List<(Domain.Entities.Product Product, int Quantity)>();
            for (int i = 0; i < productIds.Count; i++)
            {
                var product = await _productRepository.GetByIdAsync(productIds[i]);
                if (product != null)
                {
                    products.Add((product, quantities[i]));
                }
            }

            if (!products.Any())
            {
                return new ShippingCalculationResponse
                {
                    Success = false,
                    ErrorMessage = "No valid products found for shipping calculation"
                };
            }

            var (totalWeight, totalLength, totalWidth, totalHeight, totalValue) = CalculateCombinedDimensions(products);

            return await GetShippingOptionsAsync(
                destinationZipCode,
                totalWeight,
                totalLength,
                totalWidth,
                totalHeight,
                totalValue,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping for order");
            return new ShippingCalculationResponse
            {
                Success = false,
                ErrorMessage = "Error calculating shipping options"
            };
        }
    }

    public async Task<ShippingCalculationResponse> GetShippingOptionsAsync(
        string destinationZipCode,
        decimal weight,
        decimal length,
        decimal width,
        decimal height,
        decimal declaredValue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cleanZipCode = CleanZipCode(destinationZipCode);
            if (!IsValidZipCode(cleanZipCode))
            {
                return new ShippingCalculationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid destination ZIP code"
                };
            }

            var isAvailable = await _shippingService.IsServiceAvailable(cleanZipCode, cancellationToken);
            if (!isAvailable)
            {
                return new ShippingCalculationResponse
                {
                    Success = false,
                    ErrorMessage = "Shipping service not available for this location"
                };
            }

            var request = new ShippingCalculationRequest
            {
                OriginZipCode = CleanZipCode(_defaultOriginZipCode),
                DestinationZipCode = cleanZipCode,
                Weight = weight,
                Length = length,
                Width = width,
                Height = height,
                DeclaredValue = declaredValue
            };

            return await _shippingService.CalculateShippingAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipping options");
            return new ShippingCalculationResponse
            {
                Success = false,
                ErrorMessage = "Error calculating shipping options"
            };
        }
    }

    private (decimal Weight, decimal Length, decimal Width, decimal Height, decimal Value) CalculateCombinedDimensions(
        List<(Domain.Entities.Product Product, int Quantity)> products)
    {
        decimal totalWeight = 0;
        decimal totalValue = 0;
        decimal maxLength = 0;
        decimal maxWidth = 0;
        decimal totalHeight = 0;

        foreach (var (product, quantity) in products)
        {
            totalWeight += product.Weight * quantity;
            totalValue += product.Price * quantity;

            maxLength = Math.Max(maxLength, product.Length);
            maxWidth = Math.Max(maxWidth, product.Width);
            totalHeight += product.Height * quantity;
        }

        var finalLength = Math.Max(maxLength, 16);
        var finalWidth = Math.Max(maxWidth, 11);
        var finalHeight = Math.Max(totalHeight, 2);

        finalLength = Math.Min(finalLength, 100);
        finalWidth = Math.Min(finalWidth, 100);
        finalHeight = Math.Min(finalHeight, 100);

        return (totalWeight, finalLength, finalWidth, finalHeight, totalValue);
    }

    private string CleanZipCode(string zipCode)
    {
        return zipCode?.Replace("-", "").Replace(".", "").Replace(" ", "") ?? "";
    }

    private bool IsValidZipCode(string zipCode)
    {
        return !string.IsNullOrWhiteSpace(zipCode) &&
               zipCode.Length == 8 &&
               zipCode.All(char.IsDigit);
    }
}
