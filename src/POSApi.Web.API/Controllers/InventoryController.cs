using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get inventory movements history with filtering
    /// </summary>
    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<object>>> GetInventoryMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] MovementType? movementType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.InventoryMovements.AsQueryable();

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (locationId.HasValue)
            query = query.Where(m => m.LocationId == locationId.Value);

        if (movementType.HasValue)
            query = query.Where(m => m.Type == movementType.Value);

        if (startDate.HasValue)
            query = query.Where(m => m.MovementDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.MovementDate <= endDate.Value);

        var movements = await query
            .OrderByDescending(m => m.MovementDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.ProductId,
                ProductName = m.Product.Name,
                ProductSKU = m.Product.SKU,
                m.Type,
                m.Quantity,
                m.PreviousQuantity,
                m.NewQuantity,
                m.UnitCost,
                m.Reason,
                m.ReferenceNumber,
                m.ReferenceId,
                m.MovementDate,
                m.LocationId,
                LocationName = m.Location != null ? m.Location.Name : null,
                MovedByUser = m.MovedBy.Username
            })
            .ToListAsync(cancellationToken);

        return Ok(movements);
    }

    /// <summary>
    /// Get inventory movement by ID
    /// </summary>
    [HttpGet("movements/{id:guid}")]
    public async Task<ActionResult<object>> GetInventoryMovementById(Guid id, CancellationToken cancellationToken)
    {
        var movement = await _unitOfWork.Context.InventoryMovements
            .Where(m => m.Id == id)
            .Select(m => new
            {
                m.Id,
                m.ProductId,
                ProductName = m.Product.Name,
                ProductSKU = m.Product.SKU,
                m.Type,
                m.Quantity,
                m.PreviousQuantity,
                m.NewQuantity,
                m.UnitCost,
                m.Reason,
                m.ReferenceNumber,
                m.ReferenceId,
                m.MovementDate,
                m.LocationId,
                LocationName = m.Location != null ? m.Location.Name : null,
                MovedByUser = m.MovedBy.Username,
                m.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (movement == null)
            return NotFound($"Inventory movement with ID {id} not found");

        return Ok(movement);
    }

    /// <summary>
    /// Record manual inventory adjustment
    /// </summary>
    [HttpPost("movements/adjustment")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> RecordInventoryAdjustment([FromBody] InventoryAdjustmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                return NotFound($"Product with ID {request.ProductId} not found");

            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var previousQuantity = product.StockQuantity;

            // Create inventory movement record
            var movement = new InventoryMovement(
                request.ProductId,
                MovementType.Adjustment,
                request.NewQuantity - previousQuantity,
                previousQuantity,
                userId,
                request.Reason,
                request.ReferenceNumber ?? "",
                request.ReferenceId,
                request.UnitCost,
                request.LocationId
            );

            await _unitOfWork.Context.InventoryMovements.AddAsync(movement, cancellationToken);

            // Update product stock
            product.UpdateStock(request.NewQuantity);
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { MovementId = movement.Id, Message = "Inventory adjustment recorded successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get product stock level at a specific location
    /// </summary>
    [HttpGet("stock-level")]
    public async Task<ActionResult<object>> GetStockLevel(
        [FromQuery] Guid productId,
        [FromQuery] Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        if (!locationId.HasValue)
        {
            // Return total stock across all locations
            var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                return NotFound($"Product with ID {productId} not found");

            return Ok(new
            {
                ProductId = productId,
                ProductName = product.Name,
                SKU = product.SKU,
                TotalStock = product.StockQuantity,
                MinStockLevel = product.MinStockLevel,
                ReorderLevel = product.ReorderLevel,
                IsLowStock = product.IsLowStock,
                IsOutOfStock = product.IsOutOfStock,
                NeedsReorder = product.NeedsReorder
            });
        }

        // Return stock at specific location
        var productLocation = await _unitOfWork.Context.ProductLocations
            .Where(pl => pl.ProductId == productId && pl.LocationId == locationId.Value)
            .Select(pl => new
            {
                pl.ProductId,
                ProductName = pl.Product.Name,
                SKU = pl.Product.SKU,
                pl.LocationId,
                LocationName = pl.Location.Name,
                pl.Quantity,
                pl.MinStockLevel,
                pl.MaxStockLevel,
                pl.BinLocation,
                pl.IsLowStock,
                pl.IsOverStock
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (productLocation == null)
            return NotFound($"Product not found at location");

        return Ok(productLocation);
    }

    /// <summary>
    /// Get all stock levels for a product across all locations
    /// </summary>
    [HttpGet("stock-level/product/{productId:guid}/all-locations")]
    public async Task<ActionResult<IEnumerable<object>>> GetStockLevelAllLocations(Guid productId, CancellationToken cancellationToken)
    {
        var stockLevels = await _unitOfWork.Context.ProductLocations
            .Where(pl => pl.ProductId == productId && pl.IsActive)
            .Select(pl => new
            {
                pl.ProductId,
                ProductName = pl.Product.Name,
                SKU = pl.Product.SKU,
                pl.LocationId,
                LocationName = pl.Location.Name,
                LocationCode = pl.Location.Code,
                pl.Quantity,
                pl.MinStockLevel,
                pl.MaxStockLevel,
                pl.BinLocation,
                pl.IsLowStock,
                pl.IsOverStock
            })
            .ToListAsync(cancellationToken);

        return Ok(stockLevels);
    }

    /// <summary>
    /// Get inventory summary by location
    /// </summary>
    [HttpGet("summary/by-location/{locationId:guid}")]
    public async Task<ActionResult<object>> GetInventorySummaryByLocation(Guid locationId, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (location == null)
            return NotFound($"Location with ID {locationId} not found");

        var productLocations = await _unitOfWork.Context.ProductLocations
            .Where(pl => pl.LocationId == locationId && pl.IsActive)
            .ToListAsync(cancellationToken);

        var summary = new
        {
            LocationId = locationId,
            LocationName = location.Name,
            LocationCode = location.Code,
            TotalProducts = productLocations.Count,
            TotalQuantity = productLocations.Sum(pl => pl.Quantity),
            LowStockProducts = productLocations.Count(pl => pl.IsLowStock),
            OverstockProducts = productLocations.Count(pl => pl.IsOverStock),
            OutOfStockProducts = productLocations.Count(pl => pl.Quantity == 0)
        };

        return Ok(summary);
    }
}

public class InventoryAdjustmentRequest
{
    public Guid ProductId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public Guid? ReferenceId { get; set; }
    public decimal? UnitCost { get; set; }
    public Guid? LocationId { get; set; }
}
