using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Features.InterStoreTransfers.Commands.ApproveTransfer;
using POSApi.Application.Features.InterStoreTransfers.Commands.CancelTransfer;
using POSApi.Application.Features.InterStoreTransfers.Commands.CompleteTransfer;
using POSApi.Application.Features.InterStoreTransfers.Commands.CreateTransfer;
using POSApi.Application.Features.InterStoreTransfers.Commands.RejectTransfer;
using POSApi.Application.Features.InterStoreTransfers.Commands.ShipTransfer;
using POSApi.Application.Features.InterStoreTransfers.Commands.SubmitTransfer;
using POSApi.Application.Features.InterStoreTransfers.Queries.GetAllTransfers;
using POSApi.Application.Features.InterStoreTransfers.Queries.GetPendingTransfers;
using POSApi.Application.Features.InterStoreTransfers.Queries.GetTransferById;
using POSApi.Application.Features.InterStoreTransfers.Queries.GetTransfersByStore;
using POSApi.Domain.Entities;
using System.Security.Claims;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterStoreTransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public InterStoreTransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all transfers with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<InterStoreTransferDto>>> GetAllTransfers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TransferStatus? status = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = new GetAllTransfersQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            StoreId = storeId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);

        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Page", result.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(result);
    }

    /// <summary>
    /// Get transfer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InterStoreTransferDto>> GetTransferById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTransferByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Transfer with ID {id} not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Get pending transfers
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<InterStoreTransferDto>>> GetPendingTransfers(
        [FromQuery] Guid? storeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingTransfersQuery { StoreId = storeId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get transfers for a specific store
    /// </summary>
    [HttpGet("store/{storeId:guid}")]
    public async Task<ActionResult<List<InterStoreTransferDto>>> GetTransfersByStore(
        Guid storeId,
        [FromQuery] bool includeIncoming = true,
        [FromQuery] bool includeOutgoing = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransfersByStoreQuery(storeId)
        {
            IncludeIncoming = includeIncoming,
            IncludeOutgoing = includeOutgoing
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new inter-store transfer
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateTransfer(
        [FromBody] CreateInterStoreTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new CreateTransferCommand
            {
                FromStoreId = request.FromStoreId,
                ToStoreId = request.ToStoreId,
                RequestedByUserId = userId,
                Notes = request.Notes,
                Items = request.Items
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetTransferById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Submit a transfer for approval
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> SubmitTransfer(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new SubmitTransferCommand { TransferId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Approve a transfer
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ApproveTransfer(
        Guid id,
        [FromBody] ApproveTransferRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new ApproveTransferCommand
            {
                TransferId = id,
                ApprovedByUserId = userId
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
    /// Reject a transfer
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> RejectTransfer(
        Guid id,
        [FromBody] RejectTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new RejectTransferCommand
            {
                TransferId = id,
                RejectedByUserId = userId,
                Reason = request.Reason
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
    /// Ship a transfer
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ShipTransfer(
        Guid id,
        [FromBody] ShipTransferRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ShipTransferCommand { TransferId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Complete a transfer (receive goods)
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> CompleteTransfer(
        Guid id,
        [FromBody] CompleteTransferRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CompleteTransferCommand { TransferId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel a transfer
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> CancelTransfer(
        Guid id,
        [FromBody] CancelTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelTransferCommand
            {
                TransferId = id,
                Reason = request.Reason
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
