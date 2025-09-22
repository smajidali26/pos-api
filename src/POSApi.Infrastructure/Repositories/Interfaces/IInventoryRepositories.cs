using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IInventoryMovementRepository : IRepository<InventoryMovement>
{
    Task<IEnumerable<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByMovementTypeAsync(MovementType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalMovementValueAsync(Guid productId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

public interface ILocationRepository : IRepository<Location>
{
    Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetByTypeAsync(LocationType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetByParentLocationAsync(Guid parentLocationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm, CancellationToken cancellationToken = default);
}

public interface IStockCountRepository : IRepository<StockCount>
{
    Task<StockCount?> GetByCountNumberAsync(string countNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockCount>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockCount>> GetByStatusAsync(StockCountStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockCount>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockCount>> GetByCreatedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockCount>> GetScheduledCountsAsync(CancellationToken cancellationToken = default);
}

public interface IProductLocationRepository : IRepository<ProductLocation>
{
    Task<ProductLocation?> GetByProductAndLocationAsync(Guid productId, Guid locationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductLocation>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductLocation>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductLocation>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductLocation>> GetOverStockItemsAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalStockForProductAsync(Guid productId, CancellationToken cancellationToken = default);
}