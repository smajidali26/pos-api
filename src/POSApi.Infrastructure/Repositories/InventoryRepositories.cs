using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class InventoryMovementRepository : Repository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .Where(im => im.ProductId == productId)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .Where(im => im.LocationId == locationId)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByMovementTypeAsync(MovementType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .Where(im => im.Type == type)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .Where(im => im.MovementDate >= startDate && im.MovementDate <= endDate)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .Where(im => im.MovedByUserId == userId)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .Where(im => im.ReferenceNumber == referenceNumber)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalMovementValueAsync(Guid productId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(im => im.ProductId == productId && 
                        im.MovementDate >= startDate && 
                        im.MovementDate <= endDate &&
                        im.UnitCost.HasValue)
            .SumAsync(im => im.Quantity * im.UnitCost!.Value, cancellationToken);
    }

    public override async Task<InventoryMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .FirstOrDefaultAsync(im => im.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<InventoryMovement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Include(im => im.MovedBy)
            .Include(im => im.Location)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync(cancellationToken);
    }
}

public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .Include(l => l.SubLocations)
            .FirstOrDefaultAsync(l => l.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Location>> GetByTypeAsync(LocationType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .Where(l => l.Type == type)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Location>> GetByParentLocationAsync(Guid parentLocationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .Where(l => l.ParentLocationId == parentLocationId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .Where(l => l.Name.Contains(searchTerm) || 
                       l.Code.Contains(searchTerm) || 
                       l.Address.Contains(searchTerm))
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .Include(l => l.SubLocations)
            .Include(l => l.ProductLocations)
                .ThenInclude(pl => pl.Product)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.CreatedBy)
            .Include(l => l.ParentLocation)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }
}