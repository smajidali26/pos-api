using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Orders.Commands.CompleteOrder;
using POSApi.Application.Features.Orders.Commands.CreateOrder;
using POSApi.Application.Features.Orders.Commands.RefundOrderItems;
using POSApi.Application.Features.Orders.Queries.GetAllOrders;
using POSApi.Application.Features.Orders.Queries.GetOrderById;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Services;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all orders with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Application.Common.DTOs.PagedResult<OrderDto>>> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized("User ID not found in token");
        }

        // Parse user role from string to enum
        var roleString = _currentUserService.Role;
        if (string.IsNullOrEmpty(roleString) || !Enum.TryParse<UserRole>(roleString, true, out var userRole))
        {
            return Unauthorized("Invalid user role");
        }

        var query = new GetAllOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            CustomerId = customerId,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            CurrentUserId = _currentUserService.UserId.Value,
            CurrentUserRole = userRole
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
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
            // Override cashierId with the current authenticated user's ID
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized("User ID not found in token");
            }

            // Set the cashier ID from the authenticated user
            command.CashierId = _currentUserService.UserId.Value;

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

    /// <summary>
    /// Refund order items (Cashier and Manager only)
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [Authorize(Roles = "Cashier,Manager")]
    public async Task<ActionResult> RefundOrderItems(Guid id, [FromBody] RefundOrderItemsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new RefundOrderItemsCommand
            {
                OrderId = id,
                Items = request.Items.Select(i => new RefundItemDto
                {
                    OrderItemId = i.OrderItemId,
                    QuantityToRefund = i.QuantityToRefund
                }).ToList(),
                Reason = request.Reason,
                ProcessedByUserId = _currentUserService.UserId.Value
            };

            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Items refunded successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CompleteOrderRequest
{
    public POSApi.Domain.Entities.PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class RefundOrderItemsRequest
{
    public List<RefundItemRequest> Items { get; set; } = new();
    public string? Reason { get; set; }
}

public class RefundItemRequest
{
    public Guid OrderItemId { get; set; }
    public int QuantityToRefund { get; set; }
}