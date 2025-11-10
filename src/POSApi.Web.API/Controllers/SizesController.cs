using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Sizes.Commands.CreateSize;
using POSApi.Application.Features.Sizes.Commands.UpdateSize;
using POSApi.Application.Features.Sizes.Commands.DeleteSize;
using POSApi.Application.Features.Sizes.Queries.GetAllSizes;
using POSApi.Application.Features.Sizes.Queries.GetSizeById;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SizesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SizesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all sizes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SizeDto>>> GetAllSizes(CancellationToken cancellationToken)
    {
        var query = new GetAllSizesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get size by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SizeDto>> GetSizeById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSizeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Size with ID {id} not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new size
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateSize([FromBody] CreateSizeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetSizeById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing size
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSize(Guid id, [FromBody] UpdateSizeCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the request body");
        }

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a size (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSize(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteSizeCommand(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
