using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SerialNumbersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SerialNumbersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSerialNumbers(
        [FromQuery] Guid? productId = null,
        [FromQuery] SerialNumberStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? batchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.SerialNumbers.AsQueryable();

        if (productId.HasValue)
            query = query.Where(s => s.ProductId == productId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (batchId.HasValue)
            query = query.Where(s => s.BatchId == batchId.Value);

        var serialNumbers = await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.Number,
                s.ProductId,
                ProductName = s.Product.Name,
                s.BatchId,
                BatchNumber = s.Batch != null ? s.Batch.BatchNumber : null,
                s.Status,
                s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                s.SoldDate,
                s.ReturnedDate,
                s.LocationId,
                LocationName = s.Location != null ? s.Location.Name : null,
                s.WarrantyStartDate,
                s.WarrantyEndDate,
                s.IsUnderWarranty,
                s.DaysRemainingInWarranty,
                CreatedBy = s.CreatedBy.Username,
                s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(serialNumbers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetSerialNumberById(Guid id, CancellationToken cancellationToken)
    {
        var serialNumber = await _unitOfWork.Context.SerialNumbers
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Number,
                s.ProductId,
                ProductName = s.Product.Name,
                s.BatchId,
                BatchNumber = s.Batch != null ? s.Batch.BatchNumber : null,
                s.Status,
                s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                s.OrderId,
                s.SoldDate,
                s.ReturnedDate,
                s.LocationId,
                LocationName = s.Location != null ? s.Location.Name : null,
                s.WarrantyStartDate,
                s.WarrantyEndDate,
                s.WarrantyMonths,
                s.IsUnderWarranty,
                s.DaysRemainingInWarranty,
                s.Notes,
                CreatedBy = s.CreatedBy.Username,
                s.CreatedAt,
                History = s.History.Select(h => new
                {
                    h.Action,
                    ActionBy = h.ActionBy.Username,
                    h.ActionDate,
                    h.Notes
                }).OrderByDescending(h => h.ActionDate).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (serialNumber == null)
            return NotFound();

        return Ok(serialNumber);
    }

    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateSerialNumber([FromBody] CreateSerialNumberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var existing = await _unitOfWork.Context.SerialNumbers
                .FirstOrDefaultAsync(s => s.Number == request.Number, cancellationToken);

            if (existing != null)
                return BadRequest($"Serial number '{request.Number}' already exists");

            var serialNumber = new SerialNumber(
                request.Number,
                request.ProductId,
                userId,
                request.BatchId,
                request.LocationId,
                request.WarrantyMonths,
                request.Notes ?? ""
            );

            await _unitOfWork.Context.SerialNumbers.AddAsync(serialNumber, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetSerialNumberById), new { id = serialNumber.Id }, serialNumber.Id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<ActionResult> TransferSerialNumber(Guid id, [FromBody] TransferSerialNumberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var serialNumber = await _unitOfWork.Context.SerialNumbers.FindAsync(new object[] { id }, cancellationToken);
            if (serialNumber == null)
                return NotFound();

            serialNumber.Transfer(request.ToLocationId, userId, request.Reason ?? "");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Serial number transferred successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/mark-defective")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> MarkDefective(Guid id, [FromBody] MarkDefectiveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var serialNumber = await _unitOfWork.Context.SerialNumbers.FindAsync(new object[] { id }, cancellationToken);
            if (serialNumber == null)
                return NotFound();

            serialNumber.MarkAsDefective(userId, request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Serial number marked as defective" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/repair")]
    public async Task<ActionResult> RepairSerialNumber(Guid id, [FromBody] RepairSerialNumberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var serialNumber = await _unitOfWork.Context.SerialNumbers.FindAsync(new object[] { id }, cancellationToken);
            if (serialNumber == null)
                return NotFound();

            serialNumber.Repair(userId, request.Notes);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Serial number repaired successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("warranty-expiring")]
    public async Task<ActionResult<IEnumerable<object>>> GetWarrantyExpiring(
        [FromQuery] int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(daysAhead);

        var serialNumbers = await _unitOfWork.Context.SerialNumbers
            .Where(s => s.WarrantyEndDate.HasValue &&
                       s.WarrantyEndDate.Value <= expiryThreshold &&
                       s.WarrantyEndDate.Value > DateTime.UtcNow &&
                       s.Status == SerialNumberStatus.Sold)
            .Select(s => new
            {
                s.Id,
                s.Number,
                ProductName = s.Product.Name,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                s.WarrantyEndDate,
                s.DaysRemainingInWarranty
            })
            .OrderBy(s => s.WarrantyEndDate)
            .ToListAsync(cancellationToken);

        return Ok(serialNumbers);
    }
}

public class CreateSerialNumberRequest
{
    public string Number { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? LocationId { get; set; }
    public int WarrantyMonths { get; set; }
    public string? Notes { get; set; }
}

public class TransferSerialNumberRequest
{
    public Guid ToLocationId { get; set; }
    public string? Reason { get; set; }
}

public class MarkDefectiveRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RepairSerialNumberRequest
{
    public string Notes { get; set; } = string.Empty;
}
