using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockAlertsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public StockAlertsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetStockAlerts(
        [FromQuery] StockAlertStatus? status = null,
        [FromQuery] StockAlertSeverity? severity = null,
        [FromQuery] StockAlertType? alertType = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.StockAlerts.AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (severity.HasValue)
            query = query.Where(a => a.Severity == severity.Value);

        if (alertType.HasValue)
            query = query.Where(a => a.AlertType == alertType.Value);

        if (productId.HasValue)
            query = query.Where(a => a.ProductId == productId.Value);

        if (locationId.HasValue)
            query = query.Where(a => a.LocationId == locationId.Value);

        var alerts = await query
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.TriggeredDate)
            .Select(a => new
            {
                a.Id,
                a.ProductId,
                ProductName = a.Product.Name,
                ProductSKU = a.Product.SKU,
                a.LocationId,
                LocationName = a.Location != null ? a.Location.Name : null,
                a.AlertType,
                a.CurrentQuantity,
                a.ThresholdQuantity,
                a.Severity,
                a.Status,
                a.TriggeredDate,
                a.AcknowledgedDate,
                AcknowledgedBy = a.AcknowledgedBy != null ? a.AcknowledgedBy.Username : null,
                a.ResolvedDate,
                ResolvedBy = a.ResolvedBy != null ? a.ResolvedBy.Username : null,
                a.DaysActive,
                a.IsOverdue,
                a.Notes,
                a.ResolutionNotes
            })
            .ToListAsync(cancellationToken);

        return Ok(alerts);
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<ActionResult> AcknowledgeAlert(Guid id, [FromBody] AcknowledgeAlertRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var alert = await _unitOfWork.Context.StockAlerts.FindAsync(new object[] { id }, cancellationToken);
            if (alert == null)
                return NotFound();

            alert.Acknowledge(userId, request.Notes ?? "");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Alert acknowledged successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ResolveAlert(Guid id, [FromBody] ResolveAlertRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var alert = await _unitOfWork.Context.StockAlerts.FindAsync(new object[] { id }, cancellationToken);
            if (alert == null)
                return NotFound();

            alert.Resolve(userId, request.ResolutionNotes);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Alert resolved successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/dismiss")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DismissAlert(Guid id, [FromBody] DismissAlertRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var alert = await _unitOfWork.Context.StockAlerts.FindAsync(new object[] { id }, cancellationToken);
            if (alert == null)
                return NotFound();

            alert.Dismiss(userId, request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Alert dismissed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<object>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var activeAlerts = await _unitOfWork.Context.StockAlerts
            .Where(a => a.Status == StockAlertStatus.Active)
            .ToListAsync(cancellationToken);

        var summary = new
        {
            TotalActiveAlerts = activeAlerts.Count,
            CriticalAlerts = activeAlerts.Count(a => a.Severity == StockAlertSeverity.Critical),
            HighPriorityAlerts = activeAlerts.Count(a => a.Severity == StockAlertSeverity.High),
            OverdueAlerts = activeAlerts.Count(a => a.IsOverdue),
            OutOfStockAlerts = activeAlerts.Count(a => a.AlertType == StockAlertType.OutOfStock),
            LowStockAlerts = activeAlerts.Count(a => a.AlertType == StockAlertType.LowStock),
            ExpiringAlerts = activeAlerts.Count(a => a.AlertType == StockAlertType.ExpiringSoon || a.AlertType == StockAlertType.Expired)
        };

        return Ok(summary);
    }
}

public class AcknowledgeAlertRequest
{
    public string? Notes { get; set; }
}

public class ResolveAlertRequest
{
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class DismissAlertRequest
{
    public string Reason { get; set; } = string.Empty;
}
