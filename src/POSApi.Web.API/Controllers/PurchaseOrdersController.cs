using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Commands.SubmitPurchaseOrder;
using POSApi.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;
using POSApi.Application.Features.PurchaseOrders.Queries.GetOverduePurchaseOrders;
using POSApi.Application.Features.PurchaseOrders.Queries.GetPendingReceiptOrders;
using POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;
using POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrdersByStatus;
using POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrdersByVendor;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all purchase orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetAllPurchaseOrders(CancellationToken cancellationToken)
    {
        var query = new GetAllPurchaseOrdersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get purchase order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrderById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPurchaseOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound($"Purchase order with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Get purchase orders by vendor
    /// </summary>
    [HttpGet("vendor/{vendorId:guid}")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetPurchaseOrdersByVendor(Guid vendorId, CancellationToken cancellationToken)
    {
        var query = new GetPurchaseOrdersByVendorQuery(vendorId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get purchase orders by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetPurchaseOrdersByStatus(PurchaseOrderStatus status, CancellationToken cancellationToken)
    {
        var query = new GetPurchaseOrdersByStatusQuery(status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get overdue purchase orders
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetOverduePurchaseOrders(CancellationToken cancellationToken)
    {
        var query = new GetOverduePurchaseOrdersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get pending receipt orders
    /// </summary>
    [HttpGet("pending-receipt")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetPendingReceiptOrders(CancellationToken cancellationToken)
    {
        var query = new GetPendingReceiptOrdersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new purchase order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetPurchaseOrderById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Submit purchase order for approval
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult> SubmitPurchaseOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new SubmitPurchaseOrderCommand { PurchaseOrderId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Approve purchase order
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult> ApprovePurchaseOrder(Guid id, [FromBody] ApprovePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ApprovePurchaseOrderCommand 
            { 
                PurchaseOrderId = id,
                ApprovedByUserId = request.ApprovedByUserId
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
    /// Send purchase order to vendor
    /// </summary>
    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult> SendPurchaseOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new SendPurchaseOrderCommand { PurchaseOrderId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Receive purchase order items
    /// </summary>
    [HttpPost("{id:guid}/receive")]
    public async Task<ActionResult> ReceivePurchaseOrder(Guid id, [FromBody] ReceivePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ReceivePurchaseOrderCommand 
            { 
                PurchaseOrderId = id,
                ReceivedQuantities = request.ReceivedQuantities,
                ActualDeliveryDate = request.ActualDeliveryDate,
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
    /// Complete purchase order
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompletePurchaseOrder(Guid id, [FromBody] CompletePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CompletePurchaseOrderCommand 
            { 
                PurchaseOrderId = id,
                CompletionNotes = request.CompletionNotes
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
    /// Cancel purchase order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> CancelPurchaseOrder(Guid id, [FromBody] CancelPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelPurchaseOrderCommand 
            { 
                PurchaseOrderId = id,
                CancellationReason = request.CancellationReason
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

// Request DTOs
public class ApprovePurchaseOrderRequest
{
    public Guid ApprovedByUserId { get; set; }
}

public class ReceivePurchaseOrderRequest
{
    public Dictionary<Guid, int> ReceivedQuantities { get; set; } = new();
    public DateTime? ActualDeliveryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CompletePurchaseOrderRequest
{
    public string CompletionNotes { get; set; } = string.Empty;
}

public class CancelPurchaseOrderRequest
{
    public string CancellationReason { get; set; } = string.Empty;
}