using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public BatchesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all batches with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetBatches(
        [FromQuery] Guid? productId = null,
        [FromQuery] BatchStatus? status = null,
        [FromQuery] bool expiringSoon = false,
        [FromQuery] bool recalled = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.Batches.AsQueryable();

        if (productId.HasValue)
            query = query.Where(b => b.ProductId == productId.Value);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (expiringSoon)
            query = query.Where(b => b.IsExpiringSoon);

        if (recalled)
            query = query.Where(b => b.IsRecalled);

        var batches = await query
            .OrderByDescending(b => b.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                b.Id,
                b.BatchNumber,
                b.ProductId,
                ProductName = b.Product.Name,
                ProductSKU = b.Product.SKU,
                b.InitialQuantity,
                b.CurrentQuantity,
                b.ReceivedDate,
                b.ExpiryDate,
                b.ManufactureDate,
                b.VendorId,
                VendorName = b.Vendor != null ? b.Vendor.BusinessName : null,
                b.UnitCost,
                b.TotalValue,
                b.Status,
                b.IsExpired,
                b.IsExpiringSoon,
                b.DaysUntilExpiry,
                b.IsRecalled,
                b.RecallReason,
                b.RecallDate,
                CreatedBy = b.CreatedBy.Username,
                b.CreatedAt,
                b.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(batches);
    }

    /// <summary>
    /// Get batch by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetBatchById(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.Context.Batches
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.BatchNumber,
                b.ProductId,
                ProductName = b.Product.Name,
                ProductSKU = b.Product.SKU,
                b.InitialQuantity,
                b.CurrentQuantity,
                b.ReceivedDate,
                b.ExpiryDate,
                b.ManufactureDate,
                b.VendorId,
                VendorName = b.Vendor != null ? b.Vendor.BusinessName : null,
                b.UnitCost,
                b.TotalValue,
                b.Status,
                b.IsExpired,
                b.IsExpiringSoon,
                b.DaysUntilExpiry,
                b.IsRecalled,
                b.RecallReason,
                b.RecallDate,
                b.Notes,
                CreatedBy = b.CreatedBy.Username,
                b.CreatedAt,
                b.UpdatedAt,
                Movements = b.BatchMovements.Select(m => new
                {
                    m.Id,
                    m.Type,
                    m.Quantity,
                    m.PreviousQuantity,
                    m.NewQuantity,
                    m.Reason,
                    MovedBy = m.MovedBy.Username,
                    m.MovementDate
                }).OrderByDescending(m => m.MovementDate).ToList(),
                SerialNumbers = b.SerialNumbers.Select(s => new
                {
                    s.Id,
                    s.Number,
                    s.Status
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (batch == null)
            return NotFound($"Batch with ID {id} not found");

        return Ok(batch);
    }

    /// <summary>
    /// Get batches by product with FIFO order
    /// </summary>
    [HttpGet("product/{productId:guid}/fifo")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductBatchesFIFO(Guid productId, CancellationToken cancellationToken)
    {
        var batches = await _unitOfWork.Context.Batches
            .Where(b => b.ProductId == productId && b.Status == BatchStatus.Active && b.CurrentQuantity > 0)
            .OrderBy(b => b.ReceivedDate)
            .Select(b => new
            {
                b.Id,
                b.BatchNumber,
                b.CurrentQuantity,
                b.UnitCost,
                b.ReceivedDate,
                b.ExpiryDate,
                b.DaysUntilExpiry
            })
            .ToListAsync(cancellationToken);

        return Ok(batches);
    }

    /// <summary>
    /// Create a new batch
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateBatch([FromBody] CreateBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                return NotFound($"Product with ID {request.ProductId} not found");

            // Check if batch number already exists
            var existingBatch = await _unitOfWork.Context.Batches
                .FirstOrDefaultAsync(b => b.BatchNumber == request.BatchNumber, cancellationToken);

            if (existingBatch != null)
                return BadRequest($"Batch with number '{request.BatchNumber}' already exists");

            var batch = new Batch(
                request.BatchNumber,
                request.ProductId,
                request.InitialQuantity,
                request.UnitCost,
                userId,
                request.ExpiryDate,
                request.ManufactureDate,
                request.VendorId,
                request.PurchaseOrderId,
                request.Notes ?? ""
            );

            await _unitOfWork.Context.Batches.AddAsync(batch, cancellationToken);

            // Update product stock
            product.IncreaseStock(request.InitialQuantity);
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);

            // Create inventory movement
            var movement = new InventoryMovement(
                request.ProductId,
                MovementType.StockIn,
                request.InitialQuantity,
                product.StockQuantity - request.InitialQuantity,
                userId,
                $"Batch {request.BatchNumber} received",
                request.BatchNumber,
                batch.Id,
                request.UnitCost,
                null
            );

            await _unitOfWork.Context.InventoryMovements.AddAsync(movement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetBatchById), new { id = batch.Id }, batch.Id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Recall a batch
    /// </summary>
    [HttpPost("{id:guid}/recall")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> RecallBatch(Guid id, [FromBody] RecallBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var batch = await _unitOfWork.Context.Batches.FindAsync(new object[] { id }, cancellationToken);
            if (batch == null)
                return NotFound($"Batch with ID {id} not found");

            batch.Recall(request.Reason, userId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                Message = "Batch recalled successfully",
                AffectedQuantity = batch.CurrentQuantity
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark batch as expired
    /// </summary>
    [HttpPost("{id:guid}/mark-expired")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> MarkBatchExpired(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _unitOfWork.Context.Batches.FindAsync(new object[] { id }, cancellationToken);
            if (batch == null)
                return NotFound($"Batch with ID {id} not found");

            batch.MarkAsExpired();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Batch marked as expired" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update batch notes
    /// </summary>
    [HttpPut("{id:guid}/notes")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateBatchNotes(Guid id, [FromBody] UpdateNotesRequest request, CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.Context.Batches.FindAsync(new object[] { id }, cancellationToken);
        if (batch == null)
            return NotFound($"Batch with ID {id} not found");

        batch.UpdateNotes(request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Get expiring batches
    /// </summary>
    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<object>>> GetExpiringBatches(
        [FromQuery] int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(daysAhead);

        var batches = await _unitOfWork.Context.Batches
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= expiryThreshold && b.ExpiryDate.Value > DateTime.UtcNow && b.Status == BatchStatus.Active)
            .OrderBy(b => b.ExpiryDate)
            .Select(b => new
            {
                b.Id,
                b.BatchNumber,
                b.ProductId,
                ProductName = b.Product.Name,
                ProductSKU = b.Product.SKU,
                b.CurrentQuantity,
                b.UnitCost,
                b.TotalValue,
                b.ExpiryDate,
                b.DaysUntilExpiry
            })
            .ToListAsync(cancellationToken);

        return Ok(batches);
    }

    /// <summary>
    /// Get batch statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetBatchStatistics(CancellationToken cancellationToken)
    {
        var batches = await _unitOfWork.Context.Batches.ToListAsync(cancellationToken);

        var statistics = new
        {
            TotalBatches = batches.Count,
            ActiveBatches = batches.Count(b => b.Status == BatchStatus.Active),
            DepletedBatches = batches.Count(b => b.Status == BatchStatus.Depleted),
            ExpiredBatches = batches.Count(b => b.Status == BatchStatus.Expired),
            RecalledBatches = batches.Count(b => b.Status == BatchStatus.Recalled),
            BatchesExpiringSoon = batches.Count(b => b.IsExpiringSoon),
            TotalInventoryValue = batches.Where(b => b.Status == BatchStatus.Active).Sum(b => b.TotalValue),
            TotalQuantity = batches.Where(b => b.Status == BatchStatus.Active).Sum(b => b.CurrentQuantity)
        };

        return Ok(statistics);
    }
}

public class CreateBatchRequest
{
    public string BatchNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int InitialQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? Notes { get; set; }
}

public class RecallBatchRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class UpdateNotesRequest
{
    public string Notes { get; set; } = string.Empty;
}
