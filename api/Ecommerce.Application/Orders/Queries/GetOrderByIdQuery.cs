using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using MediatR;

namespace Ecommerce.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<Order?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Order?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByIdAsync(request.Id);
    }
}
