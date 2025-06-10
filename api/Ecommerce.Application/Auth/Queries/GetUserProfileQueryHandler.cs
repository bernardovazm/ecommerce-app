using Ecommerce.Domain.Repositories;
using MediatR;

namespace Ecommerce.Application.Auth.Queries;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var customer = await _userRepository.GetCustomerByIdAsync(request.UserId);
        if (customer == null)
            return null;        return new UserProfileDto(
            customer.Id,
            customer.Email,
            customer.FirstName,
            customer.LastName,
            customer.Phone,
            customer.ShippingAddress,
            customer.IsEmailConfirmed,
            customer.LastLoginAt,
            customer.CreatedAt,
            customer.Orders.Count
        );
    }
}
