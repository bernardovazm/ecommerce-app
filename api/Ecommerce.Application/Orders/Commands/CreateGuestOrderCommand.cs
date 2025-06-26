using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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
    private readonly Domain.Repositories.IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<CreateGuestOrderCommandHandler> _logger;

    public CreateGuestOrderCommandHandler(
        Domain.Repositories.IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IMessagePublisher messagePublisher,
        ILogger<CreateGuestOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
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

        order.SetShippingInfo(request.ShippingCost, request.ShippingAddress, request.ShippingService, request.ShippingDays); await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Guest order {OrderId} created successfully for {CustomerEmail}. Total: {Total}",
            order.Id, request.CustomerEmail, order.Total);

        try
        {
            await _messagePublisher.PublishOrderCreatedAsync(order.Id, cancellationToken);
            _logger.LogInformation("Order created notification sent to RabbitMQ for guest order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send order created notification to RabbitMQ for guest order {OrderId}", order.Id);
        }

        try
        {
            _logger.LogInformation("Guest order {OrderId} ready for payment processing", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify payment system for guest order {OrderId}", order.Id);
        }

        return order.Id;
    }
}
