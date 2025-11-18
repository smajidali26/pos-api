using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Inventory.Commands.RecordInventoryAdjustment;
using POSApi.Application.Features.Inventory.Queries.GetInventoryMovementById;
using POSApi.Application.Features.Inventory.Queries.GetInventoryMovements;
using POSApi.Application.Features.Inventory.Queries.GetInventorySummaryByLocation;
using POSApi.Application.Features.Inventory.Queries.GetProductStockAllLocations;
using POSApi.Application.Features.Inventory.Queries.GetStockLevel;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get inventory movements history with filtering
    /// </summary>
    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetInventoryMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] MovementType? movementType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInventoryMovementsQuery
        {
            ProductId = productId,
            LocationId = locationId,
            MovementType = movementType,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get inventory movement by ID
    /// </summary>
    [HttpGet("movements/{id:guid}")]
    public async Task<ActionResult<InventoryMovementDto>> GetInventoryMovementById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetInventoryMovementByIdQuery { MovementId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Inventory movement with ID {id} not found");

        return Ok(result);
    }

    /// <summary>
    /// Record manual inventory adjustment
    /// </summary>
    [HttpPost("movements/adjustment")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> RecordInventoryAdjustment(
        [FromBody] RecordInventoryAdjustmentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(new { MovementId = result, Message = "Inventory adjustment recorded successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get product stock level at a specific location
    /// </summary>
    [HttpGet("stock-level")]
    public async Task<ActionResult<object>> GetStockLevel(
        [FromQuery] Guid productId,
        [FromQuery] Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStockLevelQuery
        {
            ProductId = productId,
            LocationId = locationId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Product with ID {productId} not found");

        return Ok(result);
    }

    /// <summary>
    /// Get all stock levels for a product across all locations
    /// </summary>
    [HttpGet("stock-level/product/{productId:guid}/all-locations")]
    public async Task<ActionResult<IEnumerable<ProductLocationDto>>> GetStockLevelAllLocations(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductStockAllLocationsQuery { ProductId = productId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get inventory summary by location
    /// </summary>
    [HttpGet("summary/by-location/{locationId:guid}")]
    public async Task<ActionResult<InventorySummaryDto>> GetInventorySummaryByLocation(
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var query = new GetInventorySummaryByLocationQuery { LocationId = locationId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Location with ID {locationId} not found");

        return Ok(result);
    }
}
