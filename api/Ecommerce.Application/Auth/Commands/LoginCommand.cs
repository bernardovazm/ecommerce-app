using Ecommerce.Domain.ValueObjects;
using MediatR;

namespace Ecommerce.Application.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResult>;
