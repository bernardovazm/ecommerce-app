using Ecommerce.Domain.ValueObjects;
using MediatR;

namespace Ecommerce.Application.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Phone = ""
) : IRequest<AuthResult>;
