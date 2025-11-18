using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface ICustomerLoyaltyRepository : IRepository<CustomerLoyalty>
{
    Task<CustomerLoyalty?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<CustomerLoyalty>> GetByTierAsync(Guid tierId, CancellationToken cancellationToken = default);
    Task<List<CustomerLoyalty>> GetExpiringPointsAsync(int daysUntilExpiry, CancellationToken cancellationToken = default);
}
