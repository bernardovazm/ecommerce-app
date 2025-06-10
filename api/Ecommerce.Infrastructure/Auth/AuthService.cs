using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordService passwordService, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, string phone = "")
    {
        // Check if user already exists
        if (await _userRepository.ExistsAsync(email))
        {
            return AuthResult.Failure("User with this email already exists");
        }

        // Hash password
        var hashedPassword = _passwordService.HashPassword(password);

        // Create customer
        var customer = new Customer(email, hashedPassword, firstName, lastName, phone);

        // Save to database
        await _userRepository.AddCustomerAsync(customer);
        await _userRepository.SaveChangesAsync();

        // Generate tokens
        var token = _tokenService.GenerateJwtToken(customer);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _tokenService.GetTokenExpiration(token);

        return AuthResult.Success(token, refreshToken, expiresAt);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        // Find user
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return AuthResult.Failure("Invalid email or password");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            return AuthResult.Failure("Invalid email or password");
        }

        // Update last login
        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Generate tokens
        var token = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _tokenService.GetTokenExpiration(token);

        return AuthResult.Success(token, refreshToken, expiresAt);
    }    public Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // In a real implementation, you'd validate the refresh token against a database
        // For now, we'll just return a failure
        return Task.FromResult(AuthResult.Failure("Refresh token functionality not implemented yet"));
    }public Task<bool> ValidateTokenAsync(string token)
    {
        return Task.FromResult(_tokenService.ValidateToken(token));
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);
        if (userId == null)
            return null;

        return await _userRepository.GetByIdAsync(userId.Value);
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return false;

        var hashedPassword = _passwordService.HashPassword(newPassword);
        user.UpdatePassword(hashedPassword);
        
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return true;
    }
}
