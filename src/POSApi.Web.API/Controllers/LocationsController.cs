using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all locations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllLocations(
        [FromQuery] LocationType? type = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.Locations.AsQueryable();

        if (!includeInactive)
            query = query.Where(l => l.IsActive);

        if (type.HasValue)
            query = query.Where(l => l.Type == type.Value);

        var locations = await query
            .OrderBy(l => l.Name)
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.Code,
                l.Type,
                l.Address,
                l.City,
                l.State,
                l.ZipCode,
                l.FullAddress,
                l.IsActive,
                l.ParentLocationId,
                ParentLocationName = l.ParentLocation != null ? l.ParentLocation.Name : null,
                l.Notes,
                CreatedBy = l.CreatedBy.Username,
                l.CreatedAt,
                l.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(locations);
    }

    /// <summary>
    /// Get location by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetLocationById(Guid id, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations
            .Where(l => l.Id == id)
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.Code,
                l.Type,
                l.Address,
                l.City,
                l.State,
                l.ZipCode,
                l.FullAddress,
                l.IsActive,
                l.ParentLocationId,
                ParentLocationName = l.ParentLocation != null ? l.ParentLocation.Name : null,
                l.Notes,
                CreatedBy = l.CreatedBy.Username,
                l.CreatedAt,
                l.UpdatedAt,
                SubLocations = l.SubLocations.Select(sl => new
                {
                    sl.Id,
                    sl.Name,
                    sl.Code,
                    sl.Type
                }).ToList(),
                ProductCount = l.ProductLocations.Count,
                TotalQuantity = l.ProductLocations.Sum(pl => pl.Quantity)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (location == null)
            return NotFound($"Location with ID {id} not found");

        return Ok(location);
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateLocation([FromBody] CreateLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            // Check if code already exists
            var existingLocation = await _unitOfWork.Context.Locations
                .FirstOrDefaultAsync(l => l.Code == request.Code, cancellationToken);

            if (existingLocation != null)
                return BadRequest($"Location with code '{request.Code}' already exists");

            var location = new Location(
                request.Name,
                request.Code,
                request.Type,
                userId,
                request.Address ?? "",
                request.City ?? "",
                request.State ?? "",
                request.ZipCode ?? ""
            );

            if (request.ParentLocationId.HasValue)
                location.SetParentLocation(request.ParentLocationId.Value);

            if (!string.IsNullOrEmpty(request.Notes))
                location.UpdateDetails(request.Name, request.Address ?? "", request.City ?? "",
                    request.State ?? "", request.ZipCode ?? "", request.Notes);

            await _unitOfWork.Context.Locations.AddAsync(location, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, location.Id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update location details
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var location = await _unitOfWork.Context.Locations.FindAsync(new object[] { id }, cancellationToken);
            if (location == null)
                return NotFound($"Location with ID {id} not found");

            location.UpdateDetails(
                request.Name,
                request.Address ?? "",
                request.City ?? "",
                request.State ?? "",
                request.ZipCode ?? "",
                request.Notes ?? ""
            );

            if (request.ParentLocationId != location.ParentLocationId)
                location.SetParentLocation(request.ParentLocationId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Activate a location
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ActivateLocation(Guid id, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations.FindAsync(new object[] { id }, cancellationToken);
        if (location == null)
            return NotFound($"Location with ID {id} not found");

        location.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deactivate a location
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DeactivateLocation(Guid id, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations.FindAsync(new object[] { id }, cancellationToken);
        if (location == null)
            return NotFound($"Location with ID {id} not found");

        location.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Get products at a specific location
    /// </summary>
    [HttpGet("{id:guid}/products")]
    public async Task<ActionResult<IEnumerable<object>>> GetLocationProducts(
        Guid id,
        [FromQuery] bool includeLowStock = false,
        [FromQuery] bool includeOverstock = false,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.ProductLocations
            .Where(pl => pl.LocationId == id && pl.IsActive);

        if (includeLowStock)
            query = query.Where(pl => pl.IsLowStock);

        if (includeOverstock)
            query = query.Where(pl => pl.IsOverStock);

        var products = await query
            .Select(pl => new
            {
                pl.ProductId,
                ProductName = pl.Product.Name,
                SKU = pl.Product.SKU,
                Barcode = pl.Product.Barcode,
                Price = pl.Product.Price,
                pl.Quantity,
                pl.MinStockLevel,
                pl.MaxStockLevel,
                pl.BinLocation,
                pl.IsLowStock,
                pl.IsOverStock,
                ProductIsActive = pl.Product.IsActive
            })
            .OrderBy(p => p.ProductName)
            .ToListAsync(cancellationToken);

        return Ok(products);
    }
}

public class CreateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public Guid? ParentLocationId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public Guid? ParentLocationId { get; set; }
    public string? Notes { get; set; }
}
