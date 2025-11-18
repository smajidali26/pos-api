using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class CustomerLoyaltyRepository : Repository<CustomerLoyalty>, ICustomerLoyaltyRepository
{
    public CustomerLoyaltyRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<CustomerLoyalty?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cl => cl.Customer)
            .Include(cl => cl.CurrentTier)
            .Include(cl => cl.Transactions)
            .FirstOrDefaultAsync(cl => cl.CustomerId == customerId, cancellationToken);
    }

    public async Task<List<CustomerLoyalty>> GetByTierAsync(Guid tierId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cl => cl.Customer)
            .Include(cl => cl.CurrentTier)
            .Where(cl => cl.CurrentTierId == tierId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerLoyalty>> GetExpiringPointsAsync(int daysUntilExpiry, CancellationToken cancellationToken = default)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysUntilExpiry);

        return await _dbSet
            .Include(cl => cl.Customer)
            .Include(cl => cl.Transactions)
            .Where(cl => cl.Transactions.Any(t =>
                t.TransactionType == LoyaltyTransactionType.Earned &&
                t.ExpiryDate.HasValue &&
                t.ExpiryDate.Value <= expiryDate &&
                t.ExpiryDate.Value > DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }
}
