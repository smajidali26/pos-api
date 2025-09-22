using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Customers.Commands.CreateCustomer;
using POSApi.Application.Features.Customers.Queries.GetCustomerById;

namespace POS.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound($"Customer with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCustomer([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}