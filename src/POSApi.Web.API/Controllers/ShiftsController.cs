using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Features.Shifts.Commands;
using POSApi.Application.Features.Shifts.Queries;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShiftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get shifts by date range
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ShiftDto>>> GetByDateRange(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? employeeProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetShiftsByDateRangeQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.Date,
            EndDate = endDate ?? DateTime.UtcNow.Date.AddDays(30),
            StoreId = storeId,
            EmployeeProfileId = employeeProfileId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get shift by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShiftWithAttendanceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetShiftAttendanceQuery { ShiftId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get shifts for an employee
    /// </summary>
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<ActionResult<List<ShiftDto>>> GetByEmployee(
        Guid employeeId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetShiftsByEmployeeQuery
        {
            EmployeeProfileId = employeeId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get active (in-progress) shifts
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<ShiftDto>>> GetActive(CancellationToken cancellationToken)
    {
        var query = new GetActiveShiftsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get upcoming shifts for an employee
    /// </summary>
    [HttpGet("upcoming/{employeeId:guid}")]
    public async Task<ActionResult<List<ShiftDto>>> GetUpcoming(
        Guid employeeId,
        [FromQuery] int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUpcomingShiftsQuery
        {
            EmployeeProfileId = employeeId,
            DaysAhead = daysAhead
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new shift
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateShiftCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Create multiple shifts in bulk
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<List<Guid>>> CreateBulk([FromBody] BulkCreateShiftsCommand command, CancellationToken cancellationToken)
    {
        var ids = await _mediator.Send(command, cancellationToken);
        return Ok(ids);
    }

    /// <summary>
    /// Update shift schedule
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateShiftCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Clock in to a shift
    /// </summary>
    [HttpPost("{id:guid}/clock-in")]
    public async Task<ActionResult> ClockIn(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ClockInCommand { ShiftId = id, ClockInTime = DateTime.UtcNow }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Clock out from a shift
    /// </summary>
    [HttpPost("{id:guid}/clock-out")]
    public async Task<ActionResult> ClockOut(Guid id, [FromBody] ClockOutRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ClockOutCommand 
        { 
            ShiftId = id, 
            ClockOutTime = DateTime.UtcNow,
            ActualBreakMinutes = request.ActualBreakMinutes
        }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancel a shift
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> Cancel(Guid id, [FromBody] CancelShiftRequest? request = null, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new CancelShiftCommand { ShiftId = id, Reason = request?.Reason }, cancellationToken);
        return NoContent();
    }
}

public record ClockOutRequest
{
    public int ActualBreakMinutes { get; init; }
}

public record CancelShiftRequest
{
    public string? Reason { get; init; }
}
