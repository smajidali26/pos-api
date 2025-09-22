using Microsoft.EntityFrameworkCore.Storage;
using POSApi.Domain.Common;
using POSApi.Infrastructure.Repositories;
using POSApi.Infrastructure.Repositories.Interfaces;
using POSApi.Infrastructure.Services;

namespace POSApi.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PosDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private ICustomerRepository? _customers;
    private IOrderRepository? _orders;
    private IUserRepository? _users;
    private IVendorRepository? _vendors;
    private IPurchaseOrderRepository? _purchaseOrders;
    private IReturnRepository? _returns;
    private IPromotionRepository? _promotions;
    
    // Unit of Measure repositories
    private IUnitTypeRepository? _unitTypes;
    private IUnitOfMeasureRepository? _unitsOfMeasure;
    private IProductUnitRepository? _productUnits;

    public UnitOfWork(PosDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IVendorRepository Vendors => _vendors ??= new VendorRepository(_context);
    public IPurchaseOrderRepository PurchaseOrders => _purchaseOrders ??= new PurchaseOrderRepository(_context);
    public IReturnRepository Returns => _returns ??= new ReturnRepository(_context);
    public IPromotionRepository Promotions => _promotions ??= new PromotionRepository(_context);
    
    // Unit of Measure repositories
    public IUnitTypeRepository UnitTypes => _unitTypes ??= new UnitTypeRepository(_context);
    public IUnitOfMeasureRepository UnitsOfMeasure => _unitsOfMeasure ??= new UnitOfMeasureRepository(_context);
    public IProductUnitRepository ProductUnits => _productUnits ??= new ProductUnitRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);
        
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var aggregateRoots = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}