using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Vendors.Commands.CreateVendor;
using POSApi.Application.Features.Vendors.Commands.UpdateVendor;
using POSApi.Application.Features.Vendors.Commands.UpdateVendorStatus;
using POSApi.Application.Features.Vendors.Queries.GetAllVendors;
using POSApi.Application.Features.Vendors.Queries.GetVendorById;
using POSApi.Application.Features.Vendors.Queries.GetVendorsByType;
using POSApi.Application.Features.Vendors.Queries.SearchVendors;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public VendorsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all vendors
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetAllVendors([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = new GetAllVendorsQuery { IncludeInactive = includeInactive };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get vendor by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VendorDto>> GetVendorById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetVendorByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound($"Vendor with ID {id} not found");
            
        return Ok(result);
    }

    /// <summary>
    /// Search vendors by name, company, email, or contact person
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<VendorDto>>> SearchVendors([FromQuery] string searchTerm, CancellationToken cancellationToken)
    {
        var query = new SearchVendorsQuery(searchTerm);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get vendors by type
    /// </summary>
    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendorsByType(VendorType type, CancellationToken cancellationToken)
    {
        var query = new GetVendorsByTypeQuery(type);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get vendors with credit limit exceeded
    /// </summary>
    [HttpGet("credit-limit-exceeded")]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendorsWithCreditLimitExceeded(CancellationToken cancellationToken)
    {
        var vendors = await _unitOfWork.Vendors.GetVendorsWithCreditLimitExceededAsync(cancellationToken);
        
        var result = vendors.Select(v => new VendorDto
        {
            Id = v.Id,
            Name = v.Name,
            CompanyName = v.CompanyName,
            Email = v.Email,
            CreditLimit = v.CreditLimit,
            CurrentBalance = v.CurrentBalance,
            AvailableCredit = v.AvailableCredit,
            Status = v.Status,
            IsActive = v.IsActive
        });

        return Ok(result);
    }

    /// <summary>
    /// Create a new vendor
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateVendor([FromBody] CreateVendorCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetVendorById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing vendor
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateVendor(Guid id, [FromBody] UpdateVendorCommand command, CancellationToken cancellationToken)
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
    /// Update vendor status (activate, deactivate, block, etc.)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateVendorStatus(Guid id, [FromBody] UpdateVendorStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateVendorStatusCommand
            {
                VendorId = id,
                Status = request.Status,
                Reason = request.Reason
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Activate a vendor
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ActivateVendor(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateVendorStatusCommand
            {
                VendorId = id,
                Status = VendorStatus.Active
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deactivate a vendor
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DeactivateVendor(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateVendorStatusCommand
            {
                VendorId = id,
                Status = VendorStatus.Inactive
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Block a vendor with reason
    /// </summary>
    [HttpPost("{id:guid}/block")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> BlockVendor(Guid id, [FromBody] BlockVendorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateVendorStatusCommand
            {
                VendorId = id,
                Status = VendorStatus.Blocked,
                Reason = request.Reason
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get vendor types enumeration
    /// </summary>
    [HttpGet("types")]
    public ActionResult<IEnumerable<object>> GetVendorTypes()
    {
        var types = Enum.GetValues<VendorType>()
            .Select(t => new
            {
                Value = (int)t,
                Name = t.ToString(),
                DisplayName = GetVendorTypeDisplayName(t)
            });

        return Ok(types);
    }

    /// <summary>
    /// Get vendor status enumeration
    /// </summary>
    [HttpGet("statuses")]
    public ActionResult<IEnumerable<object>> GetVendorStatuses()
    {
        var statuses = Enum.GetValues<VendorStatus>()
            .Select(s => new
            {
                Value = (int)s,
                Name = s.ToString(),
                DisplayName = GetVendorStatusDisplayName(s)
            });

        return Ok(statuses);
    }

    /// <summary>
    /// Get payment terms enumeration
    /// </summary>
    [HttpGet("payment-terms")]
    public ActionResult<IEnumerable<object>> GetPaymentTerms()
    {
        var terms = Enum.GetValues<PaymentTerms>()
            .Select(pt => new
            {
                Value = (int)pt,
                Name = pt.ToString(),
                DisplayName = GetPaymentTermsDisplayName(pt)
            });

        return Ok(terms);
    }

    private static string GetVendorTypeDisplayName(VendorType type) => type switch
    {
        VendorType.Supplier => "Supplier",
        VendorType.Manufacturer => "Manufacturer",
        VendorType.Distributor => "Distributor",
        VendorType.Wholesaler => "Wholesaler",
        VendorType.ServiceProvider => "Service Provider",
        _ => type.ToString()
    };

    private static string GetVendorStatusDisplayName(VendorStatus status) => status switch
    {
        VendorStatus.Active => "Active",
        VendorStatus.Inactive => "Inactive",
        VendorStatus.Pending => "Pending Approval",
        VendorStatus.Blocked => "Blocked",
        VendorStatus.Suspended => "Suspended",
        _ => status.ToString()
    };

    private static string GetPaymentTermsDisplayName(PaymentTerms terms) => terms switch
    {
        PaymentTerms.COD => "Cash on Delivery",
        PaymentTerms.Net15 => "Net 15 Days",
        PaymentTerms.Net30 => "Net 30 Days",
        PaymentTerms.Net45 => "Net 45 Days",
        PaymentTerms.Net60 => "Net 60 Days",
        PaymentTerms.Net90 => "Net 90 Days",
        PaymentTerms.PrepaidOnly => "Prepaid Only",
        PaymentTerms.TwoTenNet30 => "2% 10 Net 30",
        _ => terms.ToString()
    };
}

// Request DTOs
public class UpdateVendorStatusRequest
{
    public VendorStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class BlockVendorRequest
{
    public string Reason { get; set; } = string.Empty;
}