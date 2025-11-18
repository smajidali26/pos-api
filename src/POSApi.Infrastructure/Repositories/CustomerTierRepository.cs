using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class CustomerTierRepository : Repository<CustomerTier>, ICustomerTierRepository
{
    public CustomerTierRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<CustomerTier?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ct => ct.Name == name, cancellationToken);
    }

    public async Task<List<CustomerTier>> GetAllOrderedBySortOrderAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(ct => ct.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
