using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Returns.Commands.CreateReturn;
using POSApi.Application.Features.Returns.Queries.GetReturnById;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get return by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReturnDto>> GetReturnById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetReturnByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound($"Return with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new return
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateReturn([FromBody] CreateReturnCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetReturnById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Process return (complete the refund)
    /// </summary>
    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult> ProcessReturn(Guid id, [FromBody] ProcessReturnRequest request, CancellationToken cancellationToken)
    {
        // This would need a ProcessReturnCommand to be implemented
        return Ok($"Process return {id} with refund method {request.RefundMethod}");
    }

    /// <summary>
    /// Get returns by original order
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<IEnumerable<ReturnDto>>> GetReturnsByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        // This would need a GetReturnsByOrderQuery to be implemented
        return Ok(new List<ReturnDto>());
    }
}

public class ProcessReturnRequest
{
    public RefundMethod RefundMethod { get; set; }
    public string Notes { get; set; } = string.Empty;
}