using Ecommerce.Application.Orders.Commands;
using Ecommerce.Application.Orders.Queries;
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
    }    [HttpGet("customer/{customerId:guid}")]
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
            return Ok(new { 
                success = true, 
                message = result.Message, 
                reference = result.Reference 
            });
        }

        if (result.IsPending)
        {
            return Accepted(new { 
                success = false, 
                pending = true, 
                message = result.Message, 
                reference = result.Reference 
            });
        }

        return BadRequest(new { 
            success = false, 
            message = result.Message 
        });
    }
}

public class ProcessPaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}
