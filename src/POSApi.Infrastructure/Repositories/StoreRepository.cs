using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class StoreRepository : Repository<Store>, IStoreRepository
{
    public StoreRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Store?> GetByCodeAsync(string storeCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ParentStore)
            .Include(s => s.Manager)
            .FirstOrDefaultAsync(s => s.Code == storeCode, cancellationToken);
    }

    public async Task<List<Store>> GetByTypeAsync(StoreType storeType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ParentStore)
            .Include(s => s.Manager)
            .Where(s => s.StoreType == storeType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Store>> GetChildStoresAsync(Guid parentStoreId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Manager)
            .Where(s => s.ParentStoreId == parentStoreId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Store?> GetWithInventoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ParentStore)
            .Include(s => s.Manager)
            .Include(s => s.StoreInventories)
                .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
