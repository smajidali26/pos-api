using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Locations.Commands.ActivateLocation;
using POSApi.Application.Features.Locations.Commands.CreateLocation;
using POSApi.Application.Features.Locations.Commands.DeactivateLocation;
using POSApi.Application.Features.Locations.Commands.UpdateLocation;
using POSApi.Application.Features.Locations.Queries.GetAllLocations;
using POSApi.Application.Features.Locations.Queries.GetLocationById;
using POSApi.Application.Features.Locations.Queries.GetLocationProducts;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all locations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetAllLocations(
        [FromQuery] LocationType? type = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllLocationsQuery
        {
            LocationType = type,
            IncludeInactive = includeInactive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get location by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationDto>> GetLocationById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetLocationByIdQuery { LocationId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Location with ID {id} not found");

        return Ok(result);
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateLocation(
        [FromBody] CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetLocationById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update location details
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateLocation(
        Guid id,
        [FromBody] UpdateLocationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            command.LocationId = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Activate a location
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ActivateLocation(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ActivateLocationCommand { LocationId = id };
            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Location activated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deactivate a location
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DeactivateLocation(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeactivateLocationCommand { LocationId = id };
            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Location deactivated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get products at a specific location
    /// </summary>
    [HttpGet("{id:guid}/products")]
    public async Task<ActionResult<IEnumerable<ProductLocationDto>>> GetLocationProducts(
        Guid id,
        [FromQuery] bool includeLowStock = false,
        [FromQuery] bool includeOverstock = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationProductsQuery
        {
            LocationId = id,
            IncludeLowStock = includeLowStock,
            IncludeOverstock = includeOverstock
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
