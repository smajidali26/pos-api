# Dependency Injection Setup - Advanced Features

## Overview
This document describes the dependency injection configuration for the three advanced features:
1. **Multi-Store Management**
2. **Advanced Analytics**
3. **Loyalty Program**

All services and repositories have been properly registered and configured.

---

## 1. Application Layer Services

**File**: `POSApi.Application\DependencyInjection.cs`

### Advanced Analytics Services
```csharp
services.AddScoped<ISalesForecastingService, SalesForecastingService>();
services.AddScoped<IABCAnalysisService, ABCAnalysisService>();
services.AddScoped<IInventoryTurnoverService, InventoryTurnoverService>();
```

**Purpose:**
- `ISalesForecastingService`: Linear Regression, Moving Average, Exponential Smoothing forecasting
- `IABCAnalysisService`: Pareto 80-15-5 analysis for inventory classification
- `IInventoryTurnoverService`: Calculate inventory turnover ratios and classify stock movement

---

## 2. Infrastructure Layer - Repositories

**File**: `POSApi.Infrastructure\DependencyInjection.cs`

### Multi-Store Management Repositories
```csharp
services.AddScoped<IStoreRepository, StoreRepository>();
services.AddScoped<IStoreInventoryRepository, StoreInventoryRepository>();
services.AddScoped<IInterStoreTransferRepository, InterStoreTransferRepository>();
```

**Repository Capabilities:**

**IStoreRepository:**
- `GetByCodeAsync(string storeCode)` - Find store by unique code
- `GetByTypeAsync(StoreType)` - Get stores by type (Headquarter, Branch, Franchise, Warehouse)
- `GetChildStoresAsync(Guid parentStoreId)` - Get all child stores in hierarchy
- `GetWithInventoryAsync(Guid id)` - Get store with all inventory items

**IStoreInventoryRepository:**
- `GetByStoreAndProductAsync(storeId, productId)` - Get specific product inventory at store
- `GetByStoreAsync(Guid storeId)` - Get all inventory for a store
- `GetLowStockItemsAsync(Guid storeId)` - Get items below reorder point

**IInterStoreTransferRepository:**
- `GetByTransferNumberAsync(string)` - Find transfer by number
- `GetByStoreAsync(Guid storeId)` - Get all transfers for a store (from or to)
- `GetByStatusAsync(InterStoreTransferStatus)` - Filter by status
- `GetWithItemsAsync(Guid id)` - Get complete transfer with all items

### Loyalty Program Repositories
```csharp
services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
services.AddScoped<ICustomerTierRepository, CustomerTierRepository>();
services.AddScoped<ICustomerLoyaltyRepository, CustomerLoyaltyRepository>();
services.AddScoped<IRewardRepository, RewardRepository>();
```

**Repository Capabilities:**

**ILoyaltyProgramRepository:**
- `GetActiveAsync()` - Get the currently active loyalty program

**ICustomerTierRepository:**
- `GetByNameAsync(string name)` - Find tier by name (Bronze, Silver, Gold, Platinum)
- `GetAllOrderedBySortOrderAsync()` - Get all tiers in ascending order

**ICustomerLoyaltyRepository:**
- `GetByCustomerIdAsync(Guid customerId)` - Get customer's loyalty account with transactions
- `GetByTierAsync(Guid tierId)` - Get all customers in a specific tier
- `GetExpiringPointsAsync(int daysUntilExpiry)` - Find customers with points expiring soon

**IRewardRepository:**
- `GetActiveRewardsAsync()` - Get all active rewards with stock available
- `GetByTypeAsync(RewardType)` - Filter by type (Discount, Product, Service, Voucher)
- `GetByPointRangeAsync(minPoints, maxPoints)` - Find rewards within point range

---

## 3. Unit of Work Updates

**Files Modified:**
- `POSApi.Infrastructure\Persistence\IUnitOfWork.cs`
- `POSApi.Infrastructure\Persistence\UnitOfWork.cs`

### Added Repository Properties

**Multi-Store Management:**
```csharp
IStoreRepository Stores { get; }
IStoreInventoryRepository StoreInventories { get; }
IInterStoreTransferRepository InterStoreTransfers { get; }
```

**Loyalty Program:**
```csharp
ILoyaltyProgramRepository LoyaltyPrograms { get; }
ICustomerTierRepository CustomerTiers { get; }
ICustomerLoyaltyRepository CustomerLoyalties { get; }
IRewardRepository Rewards { get; }
```

**Usage Example:**
```csharp
public class MyCommandHandler : IRequestHandler<MyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Unit> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByCodeAsync("HQ001", cancellationToken);
        var customerLoyalty = await _unitOfWork.CustomerLoyalties.GetByCustomerIdAsync(customerId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

---

## 4. Program.cs Configuration

**File**: `POSApi.Web.API\Program.cs`

### Response Caching and Memory Cache
```csharp
// Add Response Caching and Memory Cache
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
```

**Middleware Registration:**
```csharp
// Response Caching (must be before Authentication)
app.UseResponseCaching();

// Authentication & Authorization (Order is important!)
app.UseAuthentication();
app.UseAuthorization();
```

**Important**: Response caching middleware MUST be placed before authentication middleware.

---

## 5. Caching Strategy for Analytics

### Recommended Cache Durations

**Dashboard Endpoints:**
- Cache Duration: 5 minutes
- Reason: Balance between real-time data and performance

**Sales Analytics:**
- Cache Duration: 15 minutes
- Reason: Sales data doesn't change frequently

**Forecasts:**
- Cache Duration: 2 hours
- Reason: Forecasts are computationally expensive and don't need frequent updates

**ABC Classification:**
- Cache Duration: 24 hours
- Reason: Classification is based on historical data and changes slowly

### Applying Response Cache Attribute

**Example:**
```csharp
[HttpGet("dashboard")]
[ResponseCache(Duration = 300)] // 5 minutes
public async Task<ActionResult<DashboardDto>> GetDashboard()
{
    // ...
}

[HttpGet("sales-forecast/{productId}")]
[ResponseCache(Duration = 7200)] // 2 hours
public async Task<ActionResult<SalesForecastResult>> GetForecast(Guid productId)
{
    // ...
}
```

---

## 6. Repository Implementation Files

### Created Repository Interfaces
**Location**: `POSApi.Infrastructure\Repositories\Interfaces\`

1. `IStoreRepository.cs`
2. `IStoreInventoryRepository.cs`
3. `IInterStoreTransferRepository.cs`
4. `ILoyaltyProgramRepository.cs`
5. `ICustomerTierRepository.cs`
6. `ICustomerLoyaltyRepository.cs`
7. `IRewardRepository.cs`

### Created Repository Implementations
**Location**: `POSApi.Infrastructure\Repositories\`

1. `StoreRepository.cs`
2. `StoreInventoryRepository.cs`
3. `InterStoreTransferRepository.cs`
4. `LoyaltyProgramRepository.cs`
5. `CustomerTierRepository.cs`
6. `CustomerLoyaltyRepository.cs`
7. `RewardRepository.cs`

---

## 7. Service Lifetime Scopes

All services and repositories are registered with **Scoped** lifetime:
- Created once per HTTP request
- Shared across the request pipeline
- Disposed after request completes
- Perfect for Entity Framework DbContext operations

---

## 8. Testing the Configuration

### Verify Dependency Injection
```bash
# Build the API project
cd D:\Majid\POS\pos-api\src\POSApi.Web.API
dotnet build
```

### Check for DI Errors
Common errors to watch for:
1. **Missing registration** - Service not found in DI container
2. **Circular dependency** - Two services depend on each other
3. **Lifetime mismatches** - Singleton depending on Scoped service

### Run the Application
```bash
dotnet run
```

Check the logs for:
-  No DI-related exceptions on startup
-  All controllers resolve successfully
-  Database connection established

---

## 9. Next Steps

### Immediate Tasks:
1.  **COMPLETED**: Register all services and repositories
2.  **COMPLETED**: Add response caching configuration
3. ó **NEXT**: Set up background jobs (Hangfire) for:
   - Daily sales forecasts generation
   - Monthly ABC classification updates
   - Daily loyalty points expiry checks

### Short-Term Tasks:
1. Add `[ResponseCache]` attributes to analytics controller endpoints
2. Implement distributed caching with Redis (optional, for production)
3. Add health checks for new services
4. Configure logging for analytics operations

---

## 10. Configuration Summary

### Total Services Registered:
- **Analytics Services**: 3
- **Multi-Store Repositories**: 3
- **Loyalty Repositories**: 4
- **Total New Registrations**: 10

### Files Modified:
1. `POSApi.Application\DependencyInjection.cs` - Added analytics services
2. `POSApi.Infrastructure\DependencyInjection.cs` - Added 7 repositories
3. `POSApi.Infrastructure\Persistence\IUnitOfWork.cs` - Added 7 repository properties
4. `POSApi.Infrastructure\Persistence\UnitOfWork.cs` - Implemented 7 lazy-loaded properties
5. `POSApi.Web.API\Program.cs` - Added caching and middleware

### Files Created:
- 7 Repository interfaces
- 7 Repository implementations
- **Total**: 14 new repository files

---

## Troubleshooting

### Issue: Service not found in DI container
**Solution**: Verify the service is registered in either `Application\DependencyInjection.cs` or `Infrastructure\DependencyInjection.cs`

### Issue: DbContext disposed exception
**Solution**: Ensure repositories are registered as `Scoped`, not `Singleton`

### Issue: Circular dependency detected
**Solution**: Use `IMediator` to break circular dependencies between services

### Issue: Caching not working
**Solution**:
1. Ensure `UseResponseCaching()` is called in Program.cs
2. Verify middleware order (caching before authentication)
3. Check `[ResponseCache]` attribute is applied to controller actions

---

## Appendix: Complete DI Registration Code

### Application Layer
```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // AutoMapper
    services.AddAutoMapper(typeof(MappingProfile));

    // MediatR
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    // FluentValidation
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // Advanced Analytics Services
    services.AddScoped<ISalesForecastingService, SalesForecastingService>();
    services.AddScoped<IABCAnalysisService, ABCAnalysisService>();
    services.AddScoped<IInventoryTurnoverService, InventoryTurnoverService>();

    return services;
}
```

### Infrastructure Layer (New Repositories Only)
```csharp
// Multi-Store Management repositories
services.AddScoped<IStoreRepository, StoreRepository>();
services.AddScoped<IStoreInventoryRepository, StoreInventoryRepository>();
services.AddScoped<IInterStoreTransferRepository, InterStoreTransferRepository>();

// Loyalty Program repositories
services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
services.AddScoped<ICustomerTierRepository, CustomerTierRepository>();
services.AddScoped<ICustomerLoyaltyRepository, CustomerLoyaltyRepository>();
services.AddScoped<IRewardRepository, RewardRepository>();
```

---

**Last Updated**: 2025-11-18
**Status**:  Dependency Injection Configuration Complete
**Next Task**: Background Jobs Setup with Hangfire
