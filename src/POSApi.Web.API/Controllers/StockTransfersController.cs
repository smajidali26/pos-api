using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public StockTransfersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all stock transfers with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetStockTransfers(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? fromLocationId = null,
        [FromQuery] Guid? toLocationId = null,
        [FromQuery] TransferStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.StockTransfers.AsQueryable();

        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);

        if (fromLocationId.HasValue)
            query = query.Where(t => t.FromLocationId == fromLocationId.Value);

        if (toLocationId.HasValue)
            query = query.Where(t => t.ToLocationId == toLocationId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.RequestedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.RequestedDate <= endDate.Value);

        var transfers = await query
            .OrderByDescending(t => t.RequestedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.TransferNumber,
                t.ProductId,
                ProductName = t.Product.Name,
                ProductSKU = t.Product.SKU,
                t.FromLocationId,
                FromLocationName = t.FromLocation.Name,
                t.ToLocationId,
                ToLocationName = t.ToLocation.Name,
                t.RequestedQuantity,
                t.ShippedQuantity,
                t.ReceivedQuantity,
                t.Status,
                t.RequestedDate,
                t.ShippedDate,
                t.ReceivedDate,
                RequestedBy = t.RequestedBy.Username,
                ApprovedBy = t.ApprovedBy != null ? t.ApprovedBy.Username : null,
                t.HasVariance,
                t.Variance,
                t.VariancePercentage,
                t.TrackingNumber,
                t.ShippingCost
            })
            .ToListAsync(cancellationToken);

        return Ok(transfers);
    }

    /// <summary>
    /// Get stock transfer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetStockTransferById(Guid id, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.TransferNumber,
                t.ProductId,
                ProductName = t.Product.Name,
                ProductSKU = t.Product.SKU,
                t.FromLocationId,
                FromLocationName = t.FromLocation.Name,
                FromLocationAddress = t.FromLocation.FullAddress,
                t.ToLocationId,
                ToLocationName = t.ToLocation.Name,
                ToLocationAddress = t.ToLocation.FullAddress,
                t.RequestedQuantity,
                t.ShippedQuantity,
                t.ReceivedQuantity,
                t.Status,
                t.RequestedDate,
                t.ShippedDate,
                t.ReceivedDate,
                RequestedBy = t.RequestedBy.Username,
                RequestedByEmail = t.RequestedBy.Email,
                ApprovedBy = t.ApprovedBy != null ? t.ApprovedBy.Username : null,
                ShippedBy = t.ShippedBy != null ? t.ShippedBy.Username : null,
                ReceivedBy = t.ReceivedBy != null ? t.ReceivedBy.Username : null,
                t.Notes,
                t.ReceiverNotes,
                t.HasVariance,
                t.Variance,
                t.VariancePercentage,
                t.TrackingNumber,
                t.ShippingCost,
                t.CreatedAt,
                t.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (transfer == null)
            return NotFound($"Stock transfer with ID {id} not found");

        return Ok(transfer);
    }

    /// <summary>
    /// Create a new stock transfer request
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateStockTransfer([FromBody] CreateStockTransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                return NotFound($"Product with ID {request.ProductId} not found");

            // Validate locations exist
            var fromLocation = await _unitOfWork.Context.Locations.FindAsync(new object[] { request.FromLocationId }, cancellationToken);
            var toLocation = await _unitOfWork.Context.Locations.FindAsync(new object[] { request.ToLocationId }, cancellationToken);

            if (fromLocation == null)
                return NotFound($"From location with ID {request.FromLocationId} not found");

            if (toLocation == null)
                return NotFound($"To location with ID {request.ToLocationId} not found");

            // Check stock availability at from location
            var productLocation = await _unitOfWork.Context.ProductLocations
                .FirstOrDefaultAsync(pl => pl.ProductId == request.ProductId && pl.LocationId == request.FromLocationId, cancellationToken);

            if (productLocation == null || productLocation.Quantity < request.RequestedQuantity)
                return BadRequest($"Insufficient stock at {fromLocation.Name}. Available: {productLocation?.Quantity ?? 0}, Requested: {request.RequestedQuantity}");

            // Generate transfer number
            var transferNumber = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";

            var transfer = new StockTransfer(
                transferNumber,
                request.ProductId,
                request.FromLocationId,
                request.ToLocationId,
                request.RequestedQuantity,
                userId,
                request.Notes ?? ""
            );

            await _unitOfWork.Context.StockTransfers.AddAsync(transfer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetStockTransferById), new { id = transfer.Id }, transfer.Id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Approve a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ApproveStockTransfer(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { id }, cancellationToken);
            if (transfer == null)
                return NotFound($"Stock transfer with ID {id} not found");

            transfer.Approve(userId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Stock transfer approved successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reject a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> RejectStockTransfer(Guid id, [FromBody] RejectTransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { id }, cancellationToken);
            if (transfer == null)
                return NotFound($"Stock transfer with ID {id} not found");

            transfer.Reject(userId, request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Stock transfer rejected successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Ship a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    public async Task<ActionResult> ShipStockTransfer(Guid id, [FromBody] ShipTransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var transfer = await _unitOfWork.Context.StockTransfers
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (transfer == null)
                return NotFound($"Stock transfer with ID {id} not found");

            // Reduce stock at from location
            var fromProductLocation = await _unitOfWork.Context.ProductLocations
                .FirstOrDefaultAsync(pl => pl.ProductId == transfer.ProductId && pl.LocationId == transfer.FromLocationId, cancellationToken);

            if (fromProductLocation == null || fromProductLocation.Quantity < request.ShippedQuantity)
                return BadRequest("Insufficient stock at from location");

            transfer.Ship(userId, request.ShippedQuantity, request.TrackingNumber ?? "", request.ShippingCost);

            // Update stock at from location
            fromProductLocation.UpdateQuantity(fromProductLocation.Quantity - request.ShippedQuantity);

            // Create inventory movement
            var movement = new InventoryMovement(
                transfer.ProductId,
                MovementType.Transfer,
                request.ShippedQuantity,
                fromProductLocation.Quantity + request.ShippedQuantity,
                userId,
                $"Stock transfer {transfer.TransferNumber} shipped",
                transfer.TransferNumber,
                transfer.Id,
                null,
                transfer.FromLocationId
            );

            await _unitOfWork.Context.InventoryMovements.AddAsync(movement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Stock transfer shipped successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Receive a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/receive")]
    public async Task<ActionResult> ReceiveStockTransfer(Guid id, [FromBody] ReceiveTransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var transfer = await _unitOfWork.Context.StockTransfers
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (transfer == null)
                return NotFound($"Stock transfer with ID {id} not found");

            transfer.Receive(userId, request.ReceivedQuantity, request.ReceiverNotes ?? "");

            // Update stock at to location
            var toProductLocation = await _unitOfWork.Context.ProductLocations
                .FirstOrDefaultAsync(pl => pl.ProductId == transfer.ProductId && pl.LocationId == transfer.ToLocationId, cancellationToken);

            if (toProductLocation == null)
            {
                // Create new product location if it doesn't exist
                toProductLocation = new ProductLocation(transfer.ProductId, transfer.ToLocationId, request.ReceivedQuantity);
                await _unitOfWork.Context.ProductLocations.AddAsync(toProductLocation, cancellationToken);
            }
            else
            {
                toProductLocation.UpdateQuantity(toProductLocation.Quantity + request.ReceivedQuantity);
            }

            // Create inventory movement
            var movement = new InventoryMovement(
                transfer.ProductId,
                MovementType.StockIn,
                request.ReceivedQuantity,
                toProductLocation.Quantity - request.ReceivedQuantity,
                userId,
                $"Stock transfer {transfer.TransferNumber} received",
                transfer.TransferNumber,
                transfer.Id,
                null,
                transfer.ToLocationId
            );

            await _unitOfWork.Context.InventoryMovements.AddAsync(movement, cancellationToken);

            // Auto-complete if no variance
            if (!transfer.HasVariance)
                transfer.Complete();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                Message = "Stock transfer received successfully",
                HasVariance = transfer.HasVariance,
                Variance = transfer.Variance
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Complete a stock transfer with variance
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> CompleteStockTransfer(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { id }, cancellationToken);
            if (transfer == null)
                return NotFound($"Stock transfer with ID {id} not found");

            transfer.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Stock transfer completed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel a stock transfer
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> CancelStockTransfer(Guid id, [FromBody] CancelTransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { id }, cancellationToken);
            if (transfer == null)
                return NotFound($"Stock transfer with ID {id} not found");

            transfer.Cancel(userId, request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Stock transfer cancelled successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get transfer statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetTransferStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.StockTransfers.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.RequestedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.RequestedDate <= endDate.Value);

        var transfers = await query.ToListAsync(cancellationToken);

        var statistics = new
        {
            TotalTransfers = transfers.Count,
            PendingTransfers = transfers.Count(t => t.Status == TransferStatus.Pending),
            ApprovedTransfers = transfers.Count(t => t.Status == TransferStatus.Approved),
            InTransitTransfers = transfers.Count(t => t.Status == TransferStatus.InTransit),
            CompletedTransfers = transfers.Count(t => t.Status == TransferStatus.Completed),
            CancelledTransfers = transfers.Count(t => t.Status == TransferStatus.Cancelled),
            RejectedTransfers = transfers.Count(t => t.Status == TransferStatus.Rejected),
            TransfersWithVariance = transfers.Count(t => t.HasVariance),
            TotalQuantityTransferred = transfers.Where(t => t.Status == TransferStatus.Completed).Sum(t => t.ReceivedQuantity),
            AverageVariancePercentage = transfers.Where(t => t.HasVariance).Any()
                ? transfers.Where(t => t.HasVariance).Average(t => t.VariancePercentage)
                : 0
        };

        return Ok(statistics);
    }
}

public class CreateStockTransferRequest
{
    public Guid ProductId { get; set; }
    public Guid FromLocationId { get; set; }
    public Guid ToLocationId { get; set; }
    public int RequestedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class RejectTransferRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ShipTransferRequest
{
    public int ShippedQuantity { get; set; }
    public string? TrackingNumber { get; set; }
    public decimal? ShippingCost { get; set; }
}

public class ReceiveTransferRequest
{
    public int ReceivedQuantity { get; set; }
    public string? ReceiverNotes { get; set; }
}

public class CancelTransferRequest
{
    public string Reason { get; set; } = string.Empty;
}
