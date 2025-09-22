using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Categories.Commands.CreateCategory;
using POSApi.Application.Features.Categories.Commands.UpdateCategory;
using POSApi.Application.Features.Categories.Commands.DeleteCategory;
using POSApi.Application.Features.Categories.Queries.GetAllCategories;
using POSApi.Application.Features.Categories.Queries.GetCategoryById;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken)
    {
        var query = new GetAllCategoriesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
        {
            return NotFound($"Category with ID {id} not found");
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetCategoryById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
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
    /// Delete a category (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteCategoryCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}