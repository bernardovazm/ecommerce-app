namespace Ecommerce.Domain.ValueObjects;

public record AuthResult
{
    public bool IsSuccess { get; init; }
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }

    public static AuthResult Success(string token, string refreshToken, DateTime expiresAt) =>
        new() { IsSuccess = true, Token = token, RefreshToken = refreshToken, ExpiresAt = expiresAt };

    public static AuthResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
