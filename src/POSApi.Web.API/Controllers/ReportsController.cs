using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Features.Reports.Queries.ExportDailySalesReport;
using POSApi.Application.Features.Reports.Queries.ExportInventoryReport;
using POSApi.Application.Features.Reports.Queries.GetCustomerAnalyticsReport;
using POSApi.Application.Features.Reports.Queries.GetDailySalesReport;
using POSApi.Application.Features.Reports.Queries.GetInventoryReport;
using POSApi.Application.Features.Reports.Queries.GetReportSummary;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get daily sales report
    /// </summary>
    [HttpGet("daily-sales")]
    public async Task<ActionResult> GetDailySalesReport([FromQuery] DateTime reportDate, CancellationToken cancellationToken)
    {
        var query = new GetDailySalesReportQuery(reportDate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get inventory report
    /// </summary>
    [HttpGet("inventory")]
    public async Task<ActionResult> GetInventoryReport([FromQuery] DateTime? asOfDate, CancellationToken cancellationToken)
    {
        var query = new GetInventoryReportQuery(asOfDate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get customer analytics report
    /// </summary>
    [HttpGet("customer-analytics")]
    public async Task<ActionResult> GetCustomerAnalyticsReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        var query = new GetCustomerAnalyticsReportQuery(startDate, endDate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Export daily sales report to CSV
    /// </summary>
    [HttpGet("daily-sales/export")]
    public async Task<ActionResult> ExportDailySalesReport([FromQuery] DateTime reportDate, CancellationToken cancellationToken)
    {
        var query = new ExportDailySalesReportQuery(reportDate);
        var csvData = await _mediator.Send(query, cancellationToken);
        return File(csvData, "text/csv", $"daily-sales-report-{reportDate:yyyy-MM-dd}.csv");
    }

    /// <summary>
    /// Export inventory report to CSV
    /// </summary>
    [HttpGet("inventory/export")]
    public async Task<ActionResult> ExportInventoryReport([FromQuery] DateTime? asOfDate, CancellationToken cancellationToken)
    {
        var query = new ExportInventoryReportQuery(asOfDate);
        var csvData = await _mediator.Send(query, cancellationToken);
        var fileName = $"inventory-report-{(asOfDate ?? DateTime.Now):yyyy-MM-dd}.csv";
        return File(csvData, "text/csv", fileName);
    }

    /// <summary>
    /// Get report summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult> GetReportSummary([FromQuery] DateTime reportDate, CancellationToken cancellationToken)
    {
        var query = new GetReportSummaryQuery(reportDate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}