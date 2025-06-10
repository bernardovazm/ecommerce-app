using Ecommerce.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Infrastructure.Auth;

public class PasswordResetService : IPasswordResetService
{
    private readonly Dictionary<string, PasswordResetToken> _resetTokens = new();
    
    public Task<string> GenerateResetTokenAsync(Guid userId)
    {
        var token = GenerateSecureToken();
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        
        _resetTokens[token] = resetToken;
        
        return Task.FromResult(token);
    }

    public Task<Guid?> ValidateResetTokenAsync(string token)
    {
        if (!_resetTokens.TryGetValue(token, out var resetToken))
        {
            return Task.FromResult<Guid?>(null);
        }

        if (DateTime.UtcNow > resetToken.ExpiresAt)
        {
            _resetTokens.Remove(token);
            return Task.FromResult<Guid?>(null);
        }

        return Task.FromResult<Guid?>(resetToken.UserId);
    }

    public Task InvalidateResetTokenAsync(string token)
    {
        _resetTokens.Remove(token);
        return Task.CompletedTask;
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private class PasswordResetToken
    {
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
