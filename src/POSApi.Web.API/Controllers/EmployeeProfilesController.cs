using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Features.EmployeeProfiles.Commands;
using POSApi.Application.Features.EmployeeProfiles.Queries;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/employee-profiles")]
[Authorize]
public class EmployeeProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all employee profiles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EmployeeProfileDto>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? managerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllEmployeeProfilesQuery
        {
            Status = status,
            StoreId = storeId,
            ManagerId = managerId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get employee profile by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetEmployeeProfileByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Employee profile with ID {id} not found");

        return Ok(result);
    }

    /// <summary>
    /// Get employee profile by user ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetEmployeeProfileByUserIdQuery { UserId = userId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Employee profile for user {userId} not found");

        return Ok(result);
    }

    /// <summary>
    /// Create a new employee profile
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateEmployeeProfileCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Update employee profile
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateEmployeeProfileCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update employee compensation
    /// </summary>
    [HttpPut("{id:guid}/compensation")]
    public async Task<ActionResult> UpdateCompensation(Guid id, [FromBody] UpdateCompensationCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update employment status
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmploymentStatusCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete employee profile
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteEmployeeProfileCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}

public record GetEmployeeProfileByUserIdQuery : IRequest<EmployeeProfileDto>
{
    public Guid UserId { get; init; }
}

public record DeleteEmployeeProfileCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
