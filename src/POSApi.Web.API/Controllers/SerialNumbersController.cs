using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.SerialNumbers.Commands.CreateSerialNumber;
using POSApi.Application.Features.SerialNumbers.Commands.MarkSerialNumberDefective;
using POSApi.Application.Features.SerialNumbers.Commands.RepairSerialNumber;
using POSApi.Application.Features.SerialNumbers.Commands.TransferSerialNumber;
using POSApi.Application.Features.SerialNumbers.Queries.GetAllSerialNumbers;
using POSApi.Application.Features.SerialNumbers.Queries.GetExpiringWarranties;
using POSApi.Application.Features.SerialNumbers.Queries.GetSerialNumberById;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SerialNumbersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SerialNumbersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all serial numbers with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SerialNumberDto>>> GetSerialNumbers(
        [FromQuery] Guid? productId = null,
        [FromQuery] SerialNumberStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? batchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllSerialNumbersQuery
        {
            ProductId = productId,
            Status = status,
            CustomerId = customerId,
            BatchId = batchId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get serial number by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SerialNumberDto>> GetSerialNumberById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSerialNumberByIdQuery { SerialNumberId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Serial number with ID {id} not found");

        return Ok(result);
    }

    /// <summary>
    /// Create a new serial number
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateSerialNumber(
        [FromBody] CreateSerialNumberCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetSerialNumberById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Transfer serial number to another location
    /// </summary>
    [HttpPost("{id:guid}/transfer")]
    public async Task<ActionResult> TransferSerialNumber(
        Guid id,
        [FromBody] TransferSerialNumberCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.SerialNumberId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Serial number transferred successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark serial number as defective
    /// </summary>
    [HttpPost("{id:guid}/mark-defective")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> MarkDefective(
        Guid id,
        [FromBody] MarkSerialNumberDefectiveCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.SerialNumberId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Serial number marked as defective" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Repair a defective serial number
    /// </summary>
    [HttpPost("{id:guid}/repair")]
    public async Task<ActionResult> RepairSerialNumber(
        Guid id,
        [FromBody] RepairSerialNumberCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.SerialNumberId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Serial number repaired successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get serial numbers with warranties expiring soon
    /// </summary>
    [HttpGet("warranty-expiring")]
    public async Task<ActionResult<IEnumerable<SerialNumberDto>>> GetWarrantyExpiring(
        [FromQuery] int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExpiringWarrantiesQuery { DaysAhead = daysAhead };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
