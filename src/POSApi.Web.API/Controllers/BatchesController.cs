using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Batches.Commands.CreateBatch;
using POSApi.Application.Features.Batches.Commands.MarkBatchExpired;
using POSApi.Application.Features.Batches.Commands.RecallBatch;
using POSApi.Application.Features.Batches.Commands.UpdateBatchNotes;
using POSApi.Application.Features.Batches.Queries.GetAllBatches;
using POSApi.Application.Features.Batches.Queries.GetBatchById;
using POSApi.Application.Features.Batches.Queries.GetBatchStatistics;
using POSApi.Application.Features.Batches.Queries.GetExpiringBatches;
using POSApi.Application.Features.Batches.Queries.GetProductBatchesFIFO;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BatchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all batches with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BatchDto>>> GetBatches(
        [FromQuery] Guid? productId = null,
        [FromQuery] BatchStatus? status = null,
        [FromQuery] bool expiringSoon = false,
        [FromQuery] bool recalled = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllBatchesQuery
        {
            ProductId = productId,
            Status = status,
            ExpiringSoon = expiringSoon,
            Recalled = recalled,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get batch by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BatchDto>> GetBatchById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBatchByIdQuery { BatchId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Batch with ID {id} not found");

        return Ok(result);
    }

    /// <summary>
    /// Get batches by product with FIFO order
    /// </summary>
    [HttpGet("product/{productId:guid}/fifo")]
    public async Task<ActionResult<IEnumerable<BatchDto>>> GetProductBatchesFIFO(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductBatchesFIFOQuery { ProductId = productId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new batch
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateBatch(
        [FromBody] CreateBatchCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetBatchById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Recall a batch
    /// </summary>
    [HttpPost("{id:guid}/recall")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> RecallBatch(
        Guid id,
        [FromBody] RecallBatchCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.BatchId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Batch recalled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark batch as expired
    /// </summary>
    [HttpPost("{id:guid}/mark-expired")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> MarkBatchExpired(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new MarkBatchExpiredCommand { BatchId = id };
            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Batch marked as expired" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update batch notes
    /// </summary>
    [HttpPut("{id:guid}/notes")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateBatchNotes(
        Guid id,
        [FromBody] UpdateBatchNotesCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            command.BatchId = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get expiring batches
    /// </summary>
    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<BatchDto>>> GetExpiringBatches(
        [FromQuery] int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExpiringBatchesQuery { DaysAhead = daysAhead };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get batch statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<BatchStatisticsDto>> GetBatchStatistics(CancellationToken cancellationToken)
    {
        var query = new GetBatchStatisticsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
