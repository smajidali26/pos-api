using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IStoreRepository : IRepository<Store>
{
    Task<Store?> GetByCodeAsync(string storeCode, CancellationToken cancellationToken = default);
    Task<List<Store>> GetByTypeAsync(StoreType storeType, CancellationToken cancellationToken = default);
    Task<List<Store>> GetChildStoresAsync(Guid parentStoreId, CancellationToken cancellationToken = default);
    Task<Store?> GetWithInventoryAsync(Guid id, CancellationToken cancellationToken = default);
}
