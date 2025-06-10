using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.ValueObjects;
using MediatR;

namespace Ecommerce.Application.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResult.Failure("Email and password are required");
        }

        return await _authService.LoginAsync(request.Email, request.Password);
    }
}
