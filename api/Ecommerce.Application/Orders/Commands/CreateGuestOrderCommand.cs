using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using MediatR;

namespace Ecommerce.Application.Orders.Commands;

public record CreateGuestOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    List<OrderItemDto> Items,
    decimal ShippingCost = 0,
    string? ShippingService = null,
    int? ShippingDays = null
) : IRequest<Guid>;

public class CreateGuestOrderCommandHandler : IRequestHandler<CreateGuestOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;

    public CreateGuestOrderCommandHandler(IOrderRepository orderRepository, ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Guid> Handle(CreateGuestOrderCommand request, CancellationToken cancellationToken)
    {
        var existingCustomer = await _customerRepository.GetByEmailAsync(request.CustomerEmail);
        Customer customer;
        if (existingCustomer == null)
        {
            var nameParts = request.CustomerName.Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            customer = new Customer(request.CustomerEmail, "GUEST_TEMP_PASSWORD", firstName, lastName);
            customer.UpdateShippingAddress(request.ShippingAddress);

            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();
        }
        else
        {
            customer = existingCustomer;
        }

        var order = new Order(customer.Id); foreach (var itemDto in request.Items)
        {
            var orderItem = new OrderItem(itemDto.ProductId, itemDto.UnitPrice, itemDto.Quantity);
            order.AddItem(orderItem);
        }

        // Set shipping information
        order.SetShippingInfo(request.ShippingCost, request.ShippingAddress, request.ShippingService, request.ShippingDays);

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return order.Id;
    }
}
