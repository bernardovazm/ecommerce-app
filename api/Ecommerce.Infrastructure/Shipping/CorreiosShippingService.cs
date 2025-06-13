using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Shipping;

public class CorreiosShippingService : IShippingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CorreiosShippingService> _logger;
    private readonly CorreiosSettings _settings;

    public CorreiosShippingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CorreiosShippingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = configuration.GetSection("Correios").Get<CorreiosSettings>() ?? new CorreiosSettings();
    }

    public string GetProviderName() => "Correios"; public Task<bool> IsServiceAvailable(string zipCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = !string.IsNullOrWhiteSpace(zipCode) && zipCode.Length == 8;
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service availability for zip code {ZipCode}", zipCode);
            return Task.FromResult(false);
        }
    }

    public async Task<ShippingCalculationResponse> CalculateShippingAsync(
        ShippingCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating shipping from {Origin} to {Destination}",
                request.OriginZipCode, request.DestinationZipCode);

            var options = await CalculateMockShippingOptions(request, cancellationToken);

            return new ShippingCalculationResponse
            {
                Success = true,
                Options = options
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping");
            return new ShippingCalculationResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<List<ShippingOption>> CalculateMockShippingOptions(
        ShippingCalculationRequest request,
        CancellationToken cancellationToken)
    {

        await Task.Delay(500, cancellationToken);

        var basePrice = CalculateBasePrice(request.Weight, request.DeclaredValue);
        var distance = CalculateDistanceFactor(request.OriginZipCode, request.DestinationZipCode);

        var options = new List<ShippingOption>();

        options.Add(new ShippingOption
        {
            ServiceCode = "04510",
            ServiceName = "PAC",
            Price = Math.Round(basePrice * 0.8m * distance, 2),
            DeliveryDays = CalculateDeliveryDays(distance, 8, 12),
            Observations = "Entrega em até 12 dias úteis",
            ValidUntil = DateTime.UtcNow.AddDays(7)
        });

        options.Add(new ShippingOption
        {
            ServiceCode = "04014",
            ServiceName = "SEDEX",
            Price = Math.Round(basePrice * 1.5m * distance, 2),
            DeliveryDays = CalculateDeliveryDays(distance, 2, 5),
            Observations = "Entrega expressa",
            ValidUntil = DateTime.UtcNow.AddDays(7)
        });

        if (request.DeclaredValue >= 50)
        {
            options.Add(new ShippingOption
            {
                ServiceCode = "40215",
                ServiceName = "SEDEX 10",
                Price = Math.Round(basePrice * 2.2m * distance, 2),
                DeliveryDays = 1,
                Observations = "Entrega no próximo dia útil até 10h",
                ValidUntil = DateTime.UtcNow.AddDays(7)
            });
        }

        return options;
    }

    private decimal CalculateBasePrice(decimal weight, decimal declaredValue)
    {
        var weightPrice = weight <= 1 ? 15.00m : 15.00m + ((weight - 1) * 5.00m);
        var valuePrice = declaredValue * 0.02m;

        return Math.Max(weightPrice, 8.00m) + valuePrice;
    }

    private decimal CalculateDistanceFactor(string originZip, string destinationZip)
    {
        var originRegion = int.Parse(originZip.Substring(0, 1));
        var destRegion = int.Parse(destinationZip.Substring(0, 1));

        var regionDiff = Math.Abs(originRegion - destRegion);

        return regionDiff switch
        {
            0 => 1.0m,
            1 => 1.2m,
            2 => 1.4m,
            3 => 1.6m,
            _ => 1.8m
        };
    }

    private int CalculateDeliveryDays(decimal distanceFactor, int minDays, int maxDays)
    {
        var baseDays = (int)(minDays + (distanceFactor - 1.0m) * (maxDays - minDays));
        return Math.Max(minDays, Math.Min(maxDays, baseDays));
    }
}

public class CorreiosSettings
{
    public string? ApiUrl { get; set; } = "https://www.correios.com.br/";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string DefaultOriginZipCode { get; set; } = "01310-100";
    public bool UseMockData { get; set; } = true;
}
