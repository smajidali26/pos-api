using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Features.Commissions.Commands;
using POSApi.Application.Features.Commissions.Queries;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/commissions")]
[Authorize]
public class CommissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all commission rules
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CommissionDto>>> GetAll(
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? employeeProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllCommissionsQuery
        {
            IsActive = isActive,
            EmployeeProfileId = employeeProfileId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get commission rule by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommissionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAllCommissionsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var commission = result.FirstOrDefault(c => c.Id == id);

        if (commission == null)
            return NotFound($"Commission with ID {id} not found");

        return Ok(commission);
    }

    /// <summary>
    /// Get active commission rules
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<CommissionDto>>> GetActive(CancellationToken cancellationToken)
    {
        var query = new GetActiveCommissionsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get commission transactions for an employee
    /// </summary>
    [HttpGet("transactions/{employeeId:guid}")]
    public async Task<ActionResult<List<CommissionTransactionDto>>> GetTransactions(
        Guid employeeId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCommissionTransactionsQuery
        {
            EmployeeProfileId = employeeId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new commission rule
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCommissionCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Update commission rule
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCommissionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Activate commission rule
    /// </summary>
    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateCommissionCommand { Id = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivate commission rule
    /// </summary>
    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateCommissionCommand { Id = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Calculate commissions for an order
    /// </summary>
    [HttpPost("calculate/{orderId:guid}")]
    public async Task<ActionResult<List<Guid>>> Calculate(
        Guid orderId,
        [FromBody] CalculateCommissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CalculateOrderCommissionsCommand
        {
            OrderId = orderId,
            EmployeeProfileId = request.EmployeeProfileId,
            EmployeeRole = request.EmployeeRole
        };

        var ids = await _mediator.Send(command, cancellationToken);
        return Ok(ids);
    }

    /// <summary>
    /// Get pending commissions
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<CommissionTransactionDto>>> GetPending(
        [FromQuery] Guid? employeeProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingCommissionsQuery
        {
            EmployeeProfileId = employeeProfileId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

public record CalculateCommissionRequest
{
    public Guid EmployeeProfileId { get; init; }
    public string? EmployeeRole { get; init; }
}
