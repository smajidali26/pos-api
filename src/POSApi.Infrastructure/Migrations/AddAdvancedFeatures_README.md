# Advanced Features Migration - Complete Guide

**Migration Name:** AddAdvancedFeatures
**Created:** 2025-11-18
**Database:** SQL Server

## Overview

This migration adds comprehensive support for three major feature sets:

1. **Multi-Store Management** - Store hierarchy, inventory tracking, and inter-store transfers
2. **Advanced Analytics** - Sales forecasting, ABC classification, and inventory turnover analysis
3. **Loyalty Program** - Customer tiers, points system, rewards, and redemptions

## Tables Created

### Multi-Store Management (3 tables)
- `StoreInventory` - Per-store product inventory tracking
- `InterStoreTransfer` - Transfer requests between stores
- `InterStoreTransferItem` - Line items for transfers

### Analytics (3 tables)
- `SalesForecast` - Predictive sales forecasting data
- `ProductABCClassification` - Pareto analysis classifications
- `InventoryTurnover` - Inventory velocity metrics

### Loyalty Program (6 tables)
- `LoyaltyProgram` - Program configuration
- `CustomerTier` - Membership tiers (Bronze, Silver, Gold, Platinum)
- `CustomerLoyalty` - Customer loyalty accounts
- `LoyaltyTransaction` - Points earned/redeemed history
- `Reward` - Available rewards catalog
- `RewardRedemption` - Redemption tracking

**Total:** 12 new tables

## Prerequisites

- SQL Server 2016 or higher
- Existing POS database with `Store`, `Customer`, `Product`, `Order`, and `User` tables
- Database backup completed
- CRUD permissions on the target database

## Installation Options

### Option 1: Manual SQL Execution (Recommended)

Execute the scripts in this order:

```bash
# 1. Create tables and indexes
sqlcmd -S SERVER_NAME -d DATABASE_NAME -i AddAdvancedFeatures_Manual.sql

# 2. Insert seed data
sqlcmd -S SERVER_NAME -d DATABASE_NAME -i AddAdvancedFeatures_SeedData.sql
```

Or use SQL Server Management Studio (SSMS):
1. Open `AddAdvancedFeatures_Manual.sql`
2. Execute the script
3. Open `AddAdvancedFeatures_SeedData.sql`
4. Execute the script

### Option 2: Entity Framework CLI (When Application Layer is Fixed)

Once application layer build errors are resolved:

```bash
cd D:\Majid\POS\pos-api\src
dotnet ef migrations add AddAdvancedFeatures --project POSApi.Infrastructure --startup-project POSApi.Web.API --context PosDbContext
dotnet ef database update --project POSApi.Infrastructure --startup-project POSApi.Web.API --context PosDbContext
```

## What Gets Created

### Indexes

All tables include optimized indexes for common query patterns:

**Multi-Store:**
- Unique constraints on composite keys (StoreId + ProductId)
- Indexes on status fields for filtering
- Foreign key indexes for joins

**Analytics:**
- Composite indexes on ProductId, StoreId, Date combinations
- Classification and calculation date indexes
- Method-based filtering indexes

**Loyalty:**
- Unique customer-to-loyalty mapping
- Transaction date and type indexes
- Expiry date indexes for point management
- Reward availability indexes

### Constraints

All tables include:
- Primary keys (UNIQUEIDENTIFIER)
- Foreign keys with appropriate cascade behavior
- Check constraints for data validation
- Default values for timestamps and flags

### Seed Data

The seed data script creates:

**Customer Tiers:**
1. **Bronze** - 0+ points, 0% discount, 1x point multiplier
2. **Silver** - 500+ points, 5% discount, 1.25x multiplier
3. **Gold** - 2500+ points, 10% discount, 1.5x multiplier
4. **Platinum** - 5000+ points, 15% discount, 2x multiplier

**Default Loyalty Program:**
- Name: "Default Loyalty Program"
- Points per dollar: 1.0
- Minimum purchase: $0
- Points expiry: 365 days
- Status: Active

**Sample Rewards:**
- $5 Off - 500 points
- $10 Off - 1000 points
- $25 Cashback - 2500 points (max 4 per customer)

## Verification

After running the migration, verify success:

```sql
-- Check table creation
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    'StoreInventory', 'InterStoreTransfer', 'InterStoreTransferItem',
    'SalesForecast', 'ProductABCClassification', 'InventoryTurnover',
    'LoyaltyProgram', 'CustomerTier', 'CustomerLoyalty',
    'LoyaltyTransaction', 'Reward', 'RewardRedemption'
)
ORDER BY TABLE_NAME;

-- Check seed data
SELECT 'Customer Tiers' AS Category, COUNT(*) AS Count FROM CustomerTier
UNION ALL
SELECT 'Loyalty Programs', COUNT(*) FROM LoyaltyProgram
UNION ALL
SELECT 'Rewards', COUNT(*) FROM Reward;

-- Verify customer tier details
SELECT Name, MinPoints, MinSpend, DiscountPercentage, BenefitMultiplier, SortOrder
FROM CustomerTier
ORDER BY SortOrder;
```

Expected results:
- 12 tables created
- 4 customer tiers
- 1 loyalty program (active)
- 3 sample rewards

## Rollback

If you need to rollback the migration:

**⚠️ WARNING:** This will DELETE ALL DATA in these tables!

```bash
# Using sqlcmd
sqlcmd -S SERVER_NAME -d DATABASE_NAME -i AddAdvancedFeatures_Rollback.sql
```

Or in SSMS:
1. Open `AddAdvancedFeatures_Rollback.sql`
2. Review the script carefully
3. Execute to drop all 12 tables

## DbContext Integration

The following DbSet properties have been added to `PosDbContext.cs`:

```csharp
// Multi-Store Management
public DbSet<StoreInventory> StoreInventories => Set<StoreInventory>();
public DbSet<InterStoreTransfer> InterStoreTransfers => Set<InterStoreTransfer>();
public DbSet<InterStoreTransferItem> InterStoreTransferItems => Set<InterStoreTransferItem>();

// Analytics
public DbSet<SalesForecast> SalesForecasts => Set<SalesForecast>();
public DbSet<ProductABCClassification> ProductABCClassifications => Set<ProductABCClassification>();
public DbSet<InventoryTurnover> InventoryTurnovers => Set<InventoryTurnover>();

// Loyalty Program
public DbSet<LoyaltyProgram> LoyaltyPrograms => Set<LoyaltyProgram>();
public DbSet<CustomerTier> CustomerTiers => Set<CustomerTier>();
public DbSet<CustomerLoyalty> CustomerLoyalties => Set<CustomerLoyalty>();
public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
public DbSet<Reward> Rewards => Set<Reward>();
public DbSet<RewardRedemption> RewardRedemptions => Set<RewardRedemption>();
```

## Configuration Files Created

All Entity Framework configuration files have been created in:
`D:\Majid\POS\pos-api\src\POSApi.Infrastructure\Persistence\Configurations\`

- `StoreConfiguration.cs`
- `StoreUserConfiguration.cs`
- `StoreProductConfiguration.cs`
- `StoreInventoryConfiguration.cs`
- `InterStoreTransferConfiguration.cs`
- `InterStoreTransferItemConfiguration.cs`
- `SalesForecastConfiguration.cs`
- `ProductABCClassificationConfiguration.cs`
- `InventoryTurnoverConfiguration.cs`
- `LoyaltyProgramConfiguration.cs`
- `CustomerTierConfiguration.cs`
- `CustomerLoyaltyConfiguration.cs`
- `LoyaltyTransactionConfiguration.cs`
- `RewardConfiguration.cs`
- `RewardRedemptionConfiguration.cs`

These configurations are automatically discovered and applied via:
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosDbContext).Assembly);
```

## Key Features

### Multi-Store Management

**StoreInventory:**
- Track inventory separately per store
- Set min/max stock levels per location
- Reorder point management
- Track last restocked and sold dates

**InterStoreTransfer:**
- Request transfers between stores
- Multi-step approval workflow (Draft → Pending → Approved → InTransit → Completed)
- Track quantities at each stage (Requested, Approved, Shipped, Received)
- Audit trail with dates and responsible users
- Support for rejection and cancellation with reasons

### Analytics

**SalesForecast:**
- Multiple forecasting methods (Linear Regression, Moving Average, Exponential Smoothing)
- Confidence levels for predictions
- Store-specific or global forecasts
- Extensible metadata storage (JSON)

**ProductABCClassification:**
- Pareto analysis (80/20 rule)
- Rank products by contribution to revenue
- Calculate cumulative percentages
- Automatic classification into A, B, C tiers

**InventoryTurnover:**
- Calculate turnover ratios
- Days to sell metrics
- Average COGS and inventory value
- Classification (Fast, Normal, Slow, Dead stock)

### Loyalty Program

**Points System:**
- Configurable points per dollar
- Minimum purchase thresholds
- Automatic point expiration
- Support for bonuses and adjustments

**Customer Tiers:**
- Multiple tier levels
- Eligibility based on points or spend
- Tier-specific benefits:
  - Point multipliers
  - Discount percentages
  - Custom colors for UI

**Rewards:**
- Multiple reward types (Discount, Free Product, Cashback, Voucher)
- Time-bound validity periods
- Redemption limits (per customer and total)
- Track redemption usage

**Transaction History:**
- Complete audit trail
- Track earned, redeemed, expired, and adjusted points
- Order association for purchases
- Balance tracking (before/after)

## Business Rules Enforced

### Database Constraints

1. **Inter-Store Transfers:**
   - Cannot transfer to the same store
   - All quantities must be >= 0
   - Requested quantity must be > 0

2. **Loyalty:**
   - Points cannot be negative
   - Confidence levels between 0 and 1
   - Tier discount percentage 0-100%
   - Reward validity: ValidTo >= ValidFrom

3. **Stock Levels:**
   - All quantity fields >= 0
   - Check constraints on min/max levels

## Performance Considerations

### Indexed Queries

The following query patterns are optimized with indexes:

```sql
-- Find low stock items per store
SELECT * FROM StoreInventory
WHERE Quantity <= MinStockLevel;

-- Get pending transfers for a store
SELECT * FROM InterStoreTransfer
WHERE (FromStoreId = @StoreId OR ToStoreId = @StoreId)
AND Status = 'Pending';

-- Get customer's available rewards
SELECT r.* FROM Reward r
WHERE r.IsActive = 1
AND GETUTCDATE() BETWEEN r.ValidFrom AND r.ValidTo
AND r.PointsCost <= @CustomerPoints;

-- Find expiring points
SELECT * FROM LoyaltyTransaction
WHERE TransactionType = 'Earned'
AND ExpiryDate BETWEEN GETUTCDATE() AND DATEADD(DAY, 30, GETUTCDATE());
```

## Troubleshooting

### Common Issues

**Issue:** Foreign key constraint errors
**Solution:** Ensure Store, Customer, Product, User, and Order tables exist with data

**Issue:** Duplicate key errors on seed data
**Solution:** Seed data script checks for existing records. If you modified tier names, update the script

**Issue:** Permission denied
**Solution:** Ensure SQL user has CREATE TABLE, CREATE INDEX permissions

**Issue:** Transaction rollback
**Solution:** Check error messages. Most likely a constraint violation. Fix data and retry

### Logs and Monitoring

Each script includes PRINT statements for progress tracking:
- Table creation confirmations
- Index creation confirmations
- Seed data insertion status
- Error messages with line numbers

## Next Steps

After migration:

1. **Verify** all tables and seed data
2. **Test** application connectivity
3. **Update** API documentation
4. **Configure** loyalty program settings per business needs
5. **Train** staff on new features
6. **Monitor** performance and query patterns

## Support

For issues or questions:
1. Check verification queries above
2. Review error messages in SSMS
3. Consult entity domain models in `POSApi.Domain.Entities`
4. Check EF configurations for mapping details

## Version History

- **v1.0** (2025-11-18) - Initial migration
  - Multi-Store Management (3 tables)
  - Advanced Analytics (3 tables)
  - Loyalty Program (6 tables)
  - Seed data for tiers and program
  - Complete rollback support

---

**⚠️ Important Notes:**
- Always backup your database before running migrations
- Test in a development environment first
- Review all constraints and business rules
- Monitor performance after deployment
- Keep rollback scripts accessible for emergencies
