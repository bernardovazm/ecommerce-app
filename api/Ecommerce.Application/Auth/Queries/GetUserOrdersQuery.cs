using MediatR;
using Ecommerce.Domain.Repositories;

namespace Ecommerce.Application.Auth.Queries;

public record GetUserOrdersQuery(Guid UserId) : IRequest<List<UserOrderDto>>;

public record UserOrderDto(
    Guid Id,
    DateTime CreatedAt,
    string Status,
    decimal Total,
    List<UserOrderItemDto> Items
);

public record UserOrderItemDto(
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Total
);

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, List<UserOrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;

    public GetUserOrdersQueryHandler(IOrderRepository orderRepository, ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }

    public async Task<List<UserOrderDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.UserId);
        if (customer == null)
            return new List<UserOrderDto>();

        var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id);
        
        return orders.Select(order => new UserOrderDto(
            order.Id,
            order.CreatedAt,
            order.Status.ToString(),
            order.Items.Sum(item => item.UnitPrice * item.Quantity),
            order.Items.Select(item => new UserOrderItemDto(
                item.Product?.Name ?? "Product not found",
                item.UnitPrice,
                item.Quantity,
                item.UnitPrice * item.Quantity
            )).ToList()
        )).OrderByDescending(o => o.CreatedAt).ToList();
    }
}
