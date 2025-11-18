using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.StockAlerts.Commands.AcknowledgeAlert;
using POSApi.Application.Features.StockAlerts.Commands.DismissAlert;
using POSApi.Application.Features.StockAlerts.Commands.ResolveAlert;
using POSApi.Application.Features.StockAlerts.Queries.GetAlertDashboardSummary;
using POSApi.Application.Features.StockAlerts.Queries.GetAllStockAlerts;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockAlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockAlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all stock alerts with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts(
        [FromQuery] StockAlertStatus? status = null,
        [FromQuery] StockAlertSeverity? severity = null,
        [FromQuery] StockAlertType? alertType = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllStockAlertsQuery
        {
            Status = status,
            Severity = severity,
            AlertType = alertType,
            ProductId = productId,
            LocationId = locationId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Acknowledge a stock alert
    /// </summary>
    [HttpPost("{id:guid}/acknowledge")]
    public async Task<ActionResult> AcknowledgeAlert(
        Guid id,
        [FromBody] AcknowledgeAlertCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.AlertId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Alert acknowledged successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Resolve a stock alert
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ResolveAlert(
        Guid id,
        [FromBody] ResolveAlertCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.AlertId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Alert resolved successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Dismiss a stock alert
    /// </summary>
    [HttpPost("{id:guid}/dismiss")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DismissAlert(
        Guid id,
        [FromBody] DismissAlertCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());

            command.AlertId = id;
            command.UserId = userId;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { Message = "Alert dismissed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get stock alert dashboard summary
    /// </summary>
    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<StockAlertSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var query = new GetAlertDashboardSummaryQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
