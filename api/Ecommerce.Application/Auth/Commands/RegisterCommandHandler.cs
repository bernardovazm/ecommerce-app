using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.ValueObjects;
using MediatR;

namespace Ecommerce.Application.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return AuthResult.Failure("All required fields must be provided");
        }

        if (request.Password.Length < 6)
        {
            return AuthResult.Failure("Password must be at least 6 characters long");
        }

        if (!IsValidEmail(request.Email))
        {
            return AuthResult.Failure("Please provide a valid email address");
        }

        return await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Phone);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
