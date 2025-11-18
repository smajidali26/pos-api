using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.StockTransfers.Commands.ApproveStockTransfer;
using POSApi.Application.Features.StockTransfers.Commands.CancelStockTransfer;
using POSApi.Application.Features.StockTransfers.Commands.CreateStockTransfer;
using POSApi.Application.Features.StockTransfers.Commands.ReceiveStockTransfer;
using POSApi.Application.Features.StockTransfers.Commands.RejectStockTransfer;
using POSApi.Application.Features.StockTransfers.Commands.ShipStockTransfer;
using POSApi.Application.Features.StockTransfers.Queries.GetAllStockTransfers;
using POSApi.Application.Features.StockTransfers.Queries.GetStockTransferById;
using POSApi.Application.Features.StockTransfers.Queries.GetTransferStatistics;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockTransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all stock transfers with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransferDto>>> GetStockTransfers(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? fromLocationId = null,
        [FromQuery] Guid? toLocationId = null,
        [FromQuery] TransferStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllStockTransfersQuery
        {
            ProductId = productId,
            FromLocationId = fromLocationId,
            ToLocationId = toLocationId,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get stock transfer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StockTransferDto>> GetStockTransferById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetStockTransferByIdQuery { TransferId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Stock transfer with ID {id} not found");

        return Ok(result);
    }

    /// <summary>
    /// Create a new stock transfer request
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateStockTransfer(
        [FromBody] CreateStockTransferCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetStockTransferById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Approve a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ApproveStockTransfer(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            var command = new ApproveStockTransferCommand
            {
                TransferId = id,
                UserId = userId
            };

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Stock transfer approved successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reject a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> RejectStockTransfer(Guid id, [FromBody] RejectStockTransferCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.TransferId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Stock transfer rejected successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Ship a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ShipStockTransfer(Guid id, [FromBody] ShipStockTransferCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.TransferId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Stock transfer shipped successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Receive a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ReceiveStockTransfer(Guid id, [FromBody] ReceiveStockTransferCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.TransferId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Stock transfer received successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> CancelStockTransfer(Guid id, [FromBody] CancelStockTransferCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.TransferId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Stock transfer cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get stock transfer statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<StockTransferStatisticsDto>> GetStatistics(CancellationToken cancellationToken)
    {
        var query = new GetTransferStatisticsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
