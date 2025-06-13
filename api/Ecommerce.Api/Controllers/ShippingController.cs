using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly IShippingCalculationService _shippingCalculationService;
    private readonly ILogger<ShippingController> _logger;

    public ShippingController(
        IShippingCalculationService shippingCalculationService,
        ILogger<ShippingController> logger)
    {
        _shippingCalculationService = shippingCalculationService;
        _logger = logger;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateShipping([FromBody] ShippingCalculationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _shippingCalculationService.CalculateShippingForOrderAsync(
                request.ProductIds,
                request.Quantities,
                request.DestinationZipCode);

            if (!response.Success)
            {
                return BadRequest(new { error = response.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                options = response.Options.Select(o => new
                {
                    serviceCode = o.ServiceCode,
                    serviceName = o.ServiceName,
                    price = o.Price,
                    deliveryDays = o.DeliveryDays,
                    observations = o.Observations,
                    validUntil = o.ValidUntil
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("calculate-simple")]
    public async Task<IActionResult> CalculateSimpleShipping([FromBody] SimpleShippingRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _shippingCalculationService.GetShippingOptionsAsync(
                request.DestinationZipCode,
                request.Weight,
                request.Length,
                request.Width,
                request.Height,
                request.DeclaredValue);

            if (!response.Success)
            {
                return BadRequest(new { error = response.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                options = response.Options.Select(o => new
                {
                    serviceCode = o.ServiceCode,
                    serviceName = o.ServiceName,
                    price = o.Price,
                    deliveryDays = o.DeliveryDays,
                    observations = o.Observations,
                    validUntil = o.ValidUntil
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating simple shipping");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("zip-code/{zipCode}/validate")]
    public async Task<IActionResult> ValidateZipCode(string zipCode)
    {
        try
        {
            await Task.CompletedTask;
            var cleanZipCode = zipCode?.Replace("-", "").Replace(".", "").Replace(" ", "") ?? "";

            if (string.IsNullOrWhiteSpace(cleanZipCode) || cleanZipCode.Length != 8 || !cleanZipCode.All(char.IsDigit))
            {
                return Ok(new { valid = false, message = "Invalid ZIP code format" });
            }

            return Ok(new { valid = true, zipCode = cleanZipCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ZIP code {ZipCode}", zipCode);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class ShippingCalculationRequestDto
{
    public List<Guid> ProductIds { get; set; } = new();
    public List<int> Quantities { get; set; } = new();
    public string DestinationZipCode { get; set; } = string.Empty;
}

public class SimpleShippingRequestDto
{
    public string DestinationZipCode { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal DeclaredValue { get; set; }
}
