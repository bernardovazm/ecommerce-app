using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using MediatR;

namespace Ecommerce.Application.Orders.Queries;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<IEnumerable<Order>>;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<Order>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<Order>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByCustomerIdAsync(request.CustomerId);
    }
}
