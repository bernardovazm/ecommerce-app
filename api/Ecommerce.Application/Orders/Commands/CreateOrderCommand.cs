using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Orders.Commands;

public record CreateOrderCommand(Guid CustomerId, List<OrderItemDto> Items) : IRequest<Guid>;

public record OrderItemDto(Guid ProductId, decimal UnitPrice, int Quantity);

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly Domain.Repositories.IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        Domain.Repositories.IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IMessagePublisher messagePublisher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
            throw new ArgumentException("Customer not found");

        var order = new Order(request.CustomerId);

        foreach (var itemDto in request.Items)
        {
            var orderItem = new OrderItem(itemDto.ProductId, itemDto.UnitPrice, itemDto.Quantity);
            order.AddItem(orderItem);
        }
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} created successfully. Total: {Total}", order.Id, order.Total);

        try
        {
            await _messagePublisher.PublishOrderCreatedAsync(order.Id, cancellationToken);
            _logger.LogInformation("Order created notification sent to RabbitMQ for order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send order created notification to RabbitMQ for order {OrderId}", order.Id);
        }

        try
        {
            _logger.LogInformation("Order {OrderId} ready for payment processing", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify payment system for order {OrderId}", order.Id);
        }

        return order.Id;
    }
}
