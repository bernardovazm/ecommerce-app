using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, string phone = "");
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<User?> GetUserFromTokenAsync(string token);
    Task<bool> ResetPasswordAsync(string email, string newPassword);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    DateTime GetTokenExpiration(string token);
}

public interface IPasswordResetService
{
    Task<string> GenerateResetTokenAsync(Guid userId);
    Task<Guid?> ValidateResetTokenAsync(string token);
    Task InvalidateResetTokenAsync(string token);
}
