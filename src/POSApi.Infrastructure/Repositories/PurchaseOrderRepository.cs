using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Include(po => po.Items)
            .Where(po => po.VendorId == vendorId)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Where(po => po.Status == status)
            .OrderBy(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetOverduePurchaseOrdersAsync(CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Where(po => po.ExpectedDeliveryDate.HasValue && 
                        po.ExpectedDeliveryDate < currentDate &&
                        po.Status != PurchaseOrderStatus.Received &&
                        po.Status != PurchaseOrderStatus.Completed &&
                        po.Status != PurchaseOrderStatus.Cancelled)
            .OrderBy(po => po.ExpectedDeliveryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetPendingReceiptAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .Where(po => po.Status == PurchaseOrderStatus.Sent || 
                        po.Status == PurchaseOrderStatus.PartiallyReceived)
            .OrderBy(po => po.ExpectedDeliveryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByCreatedUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Where(po => po.CreatedByUserId == userId)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public override async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(po => po.Vendor)
            .Include(po => po.CreatedBy)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }
}