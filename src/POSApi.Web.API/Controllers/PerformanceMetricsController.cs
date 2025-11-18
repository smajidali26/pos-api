using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Features.PerformanceMetrics.Commands;
using POSApi.Application.Features.PerformanceMetrics.Queries;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/performance")]
[Authorize]
public class PerformanceMetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PerformanceMetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get performance metrics for an employee
    /// </summary>
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<ActionResult<List<PerformanceMetricDto>>> GetEmployeePerformance(
        Guid employeeId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmployeePerformanceQuery
        {
            EmployeeProfileId = employeeId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get performance metric by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PerformanceMetricDto>> GetById(
        Guid id,
        [FromQuery] Guid employeeProfileId,
        CancellationToken cancellationToken)
    {
        var query = new GetEmployeePerformanceQuery { EmployeeProfileId = employeeProfileId };
        var result = await _mediator.Send(query, cancellationToken);
        var metric = result.FirstOrDefault(m => m.Id == id);

        if (metric == null)
            return NotFound($"Performance metric with ID {id} not found");

        return Ok(metric);
    }

    /// <summary>
    /// Get performance metrics for a specific period
    /// </summary>
    [HttpGet("period")]
    public async Task<ActionResult<List<PerformanceMetricDto>>> GetByPeriod(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] Guid? employeeProfileId = null,
        CancellationToken cancellationToken = default)
    {
        if (employeeProfileId.HasValue)
        {
            var query = new GetEmployeePerformanceQuery
            {
                EmployeeProfileId = employeeProfileId.Value,
                StartDate = periodStart,
                EndDate = periodEnd
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        // Get all employees' performance for the period
        var topQuery = new GetTopPerformersQuery
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Count = 1000 // Large number to get all
        };

        var topResult = await _mediator.Send(topQuery, cancellationToken);
        return Ok(topResult);
    }

    /// <summary>
    /// Get top performers
    /// </summary>
    [HttpGet("top-performers")]
    public async Task<ActionResult<List<PerformanceMetricDto>>> GetTopPerformers(
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTopPerformersQuery
        {
            PeriodStart = periodStart ?? DateTime.UtcNow.AddMonths(-1).Date,
            PeriodEnd = periodEnd ?? DateTime.UtcNow.Date,
            Count = count
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Compare performance between employees
    /// </summary>
    [HttpGet("comparison")]
    public async Task<ActionResult<List<PerformanceMetricDto>>> GetComparison(
        [FromQuery] List<Guid> employeeProfileIds,
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPerformanceComparisonQuery
        {
            EmployeeProfileIds = employeeProfileIds,
            PeriodStart = periodStart ?? DateTime.UtcNow.AddMonths(-1).Date,
            PeriodEnd = periodEnd ?? DateTime.UtcNow.Date
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get performance trends for an employee
    /// </summary>
    [HttpGet("trends/{employeeId:guid}")]
    public async Task<ActionResult<PerformanceTrendDto>> GetTrends(
        Guid employeeId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPerformanceTrendsQuery
        {
            EmployeeProfileId = employeeId,
            StartDate = startDate ?? DateTime.UtcNow.AddMonths(-6).Date,
            EndDate = endDate ?? DateTime.UtcNow.Date
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calculate performance metrics for an employee
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<Guid>> Calculate([FromBody] CalculatePerformanceMetricsCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    /// <summary>
    /// Recalculate all employee metrics for a period
    /// </summary>
    [HttpPost("recalculate-all")]
    public async Task<ActionResult<int>> RecalculateAll([FromBody] RecalculateAllMetricsCommand command, CancellationToken cancellationToken)
    {
        var count = await _mediator.Send(command, cancellationToken);
        return Ok(new { calculatedCount = count });
    }
}
