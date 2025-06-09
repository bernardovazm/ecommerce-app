
namespace Ecommerce.Domain.ValueObjects;
public record PaymentResult(bool Success, string? GatewayId, string? Error);