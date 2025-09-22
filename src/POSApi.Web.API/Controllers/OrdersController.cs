using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Orders.Commands.CompleteOrder;
using POSApi.Application.Features.Orders.Commands.CreateOrder;
using POSApi.Application.Features.Orders.Queries.GetOrderById;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound($"Order with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetOrderById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Complete an order
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompleteOrder(Guid id, [FromBody] CompleteOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CompleteOrderCommand 
            { 
                OrderId = id, 
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes
            };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class CompleteOrderRequest
{
    public POSApi.Domain.Entities.PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}