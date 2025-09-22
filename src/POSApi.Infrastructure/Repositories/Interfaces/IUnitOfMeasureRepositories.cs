using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IUnitTypeRepository : IRepository<UnitType>
{
    Task<UnitType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitType>> GetOrderedAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfMeasureRepository : IRepository<UnitOfMeasure>
{
    Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> GetByUnitTypeAsync(Guid unitTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> GetByUnitTypeNameAsync(string unitTypeName, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> GetBaseUnitsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> GetActiveByUnitTypeAsync(Guid unitTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasure>> GetOrderedByTypeAsync(Guid unitTypeId, CancellationToken cancellationToken = default);
}

public interface IProductUnitRepository : IRepository<ProductUnit>
{
    Task<ProductUnit?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductUnit>> GetByBaseUnitAsync(Guid baseUnitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductUnit>> GetByPackagingUnitAsync(Guid packagingUnitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductUnit>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductUnit>> GetWithPhysicalPropertiesAsync(CancellationToken cancellationToken = default);
}