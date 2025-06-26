using Ecommerce.Application.Orders.Commands;
using Ecommerce.Application.Orders.Queries;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var orderId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { orderId });
    }

    [HttpPost("guest")]
    public async Task<IActionResult> CreateGuestOrder([FromBody] CreateGuestOrderCommand command)
    {
        var orderId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { orderId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id));
        if (order == null)
            return NotFound();

        return Ok(order);
    }
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var orders = await mediator.Send(new GetOrdersByCustomerQuery(customerId));
        return Ok(orders);
    }

    [HttpPost("{id:guid}/process-payment")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest request)
    {
        var command = new ProcessOrderPaymentCommand
        {
            OrderId = id,
            PaymentMethod = request.PaymentMethod,
            CustomerEmail = request.CustomerEmail
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ProcessOrderPaymentHandler>();
        var result = await handler.HandleAsync(command);

        if (result.IsSuccess)
        {
            return Ok(new
            {
                success = true,
                message = result.Message,
                reference = result.Reference
            });
        }

        if (result.IsPending)
        {
            return Accepted(new
            {
                success = false,
                pending = true,
                message = result.Message,
                reference = result.Reference
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message
        });
    }

    [HttpPost("{id:guid}/test-queue")]
    public async Task<IActionResult> TestQueue(Guid id)
    {
        try
        {
            var order = await mediator.Send(new GetOrderByIdQuery(id));
            if (order == null)
                return NotFound("Order not found");

            var messagePublisher = HttpContext.RequestServices.GetRequiredService<IMessagePublisher>();

            var paymentRequestRepository = HttpContext.RequestServices.GetRequiredService<IPaymentRequestRepository>();
            var paymentRequest = new PaymentRequest(id, order.Total, "credit_card", "test@example.com");

            await paymentRequestRepository.CreateAsync(paymentRequest);
            await messagePublisher.PublishPaymentRequestAsync(paymentRequest.Id);

            return Ok(new
            {
                success = true,
                message = "Payment request sent to queue successfully",
                paymentRequestId = paymentRequest.Id,
                orderId = id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = $"Error testing queue: {ex.Message}"
            });
        }
    }
    [HttpGet("integration-status")]
    public IActionResult GetIntegrationStatus()
    {
        try
        {
            var messagePublisher = HttpContext.RequestServices.GetRequiredService<IMessagePublisher>();
            var paymentService = HttpContext.RequestServices.GetRequiredService<IPaymentService>();

            return Ok(new
            {
                status = "integrated",
                message = "Order system is fully integrated with payment queue",
                features = new
                {
                    orderCreation = true,
                    paymentQueue = true,
                    paymentProcessing = true,
                    rabbitMQIntegration = messagePublisher != null,
                    paymentServiceIntegration = paymentService != null
                },
                instructions = new
                {
                    createOrder = "POST /api/orders or /api/orders/guest",
                    processPayment = "POST /api/orders/{id}/process-payment",
                    testQueue = "POST /api/orders/{id}/test-queue",
                    checkStatus = "GET /api/orders/integration-status"
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                status = "error",
                message = ex.Message,
                features = new
                {
                    orderCreation = true,
                    paymentQueue = false,
                    paymentProcessing = false,
                    rabbitMQIntegration = false,
                    paymentServiceIntegration = false
                }
            });
        }
    }
}

public class ProcessPaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}
