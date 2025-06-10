using Ecommerce.Domain.Entities;
using MediatR;

namespace Ecommerce.Application.Auth.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string ShippingAddress,
    bool IsEmailConfirmed,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    int TotalOrders
);
