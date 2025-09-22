using POSApi.Domain.Common;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Persistence;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    IVendorRepository Vendors { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }
    IReturnRepository Returns { get; }
    IPromotionRepository Promotions { get; }
    
    // Unit of Measure repositories
    IUnitTypeRepository UnitTypes { get; }
    IUnitOfMeasureRepository UnitsOfMeasure { get; }
    IProductUnitRepository ProductUnits { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default);
}