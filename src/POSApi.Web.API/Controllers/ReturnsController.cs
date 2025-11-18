using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Features.Returns.Commands.CreateReturn;
using POSApi.Application.Features.Returns.Commands.ProcessReturn;
using POSApi.Application.Features.Returns.Queries.GetReturnById;
using POSApi.Application.Features.Returns.Queries.GetReturnsByOrder;
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
        try
        {
            var command = new ProcessReturnCommand
            {
                ReturnId = id,
                RefundMethod = request.RefundMethod,
                Notes = request.Notes
            };

            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Return processed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get returns by original order
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<IEnumerable<ReturnDto>>> GetReturnsByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetReturnsByOrderQuery(orderId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}