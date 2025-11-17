# Database Migration Guide - Advanced Inventory Management

## Overview
This guide provides instructions for creating and applying the database migration for the advanced inventory management features.

## Prerequisites
- .NET SDK installed
- Entity Framework Core tools installed (install if missing: `dotnet tool install --global dotnet-ef`)
- Access to the database server

### Installing EF Core Tools (If Not Already Installed)
```bash
dotnet tool install --global dotnet-ef
# Or update if already installed
dotnet tool update --global dotnet-ef
```

## Migration Commands

### 1. Navigate to Infrastructure Project
```bash
cd D:\Majid\POS\pos-api\src\POSApi.Infrastructure
```

### 2. Create Migration
```bash
dotnet ef migrations add AddAdvancedInventoryManagement --startup-project ../POSApi.Web.API
```

### 3. Review Migration Files
The migration will create files in `POSApi.Infrastructure/Persistence/Migrations/` directory.
Review these files to ensure they match the expected schema changes.

### 4. Apply Migration to Database
```bash
dotnet ef database update --startup-project ../POSApi.Web.API
```

## Alternative: Using Package Manager Console (Visual Studio)

If you prefer using Visual Studio's Package Manager Console:

```powershell
# Set default project to POSApi.Infrastructure
Add-Migration AddAdvancedInventoryManagement -StartupProject POSApi.Web.API
Update-Database -StartupProject POSApi.Web.API
```

## What Gets Created

The migration will create the following tables:

### 1. StockTransfers
- Transfer workflow management
- Tracks transfers between locations
- Supports variance tracking

### 2. Batches
- Batch/lot tracking
- Expiry date management
- FIFO support

### 3. BatchMovements
- Batch quantity changes history
- Links to inventory movements

### 4. SerialNumbers
- Individual item tracking
- Warranty management
- Status lifecycle

### 5. SerialNumberHistories
- Complete serial number audit trail
- Tracks all status changes

### 6. StockAlerts
- Alert management system
- Multi-severity alerts
- Alert lifecycle tracking

### 7. InventoryValuations
- Inventory costing methods (FIFO/LIFO/Weighted Average)
- Product-level valuation tracking

### 8. InventoryValuationLayers
- Cost layers for FIFO/LIFO calculations
- Tracks layer consumption

### Existing Tables (Updates)
- **InventoryMovements**: Already exists, now fully utilized
- **Locations**: Already exists, now fully utilized
- **ProductLocations**: Already exists, now fully utilized

## Verification

After applying the migration, verify the tables were created:

### Using SQL Server Management Studio
```sql
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    'StockTransfers',
    'Batches',
    'BatchMovements',
    'SerialNumbers',
    'SerialNumberHistories',
    'StockAlerts',
    'InventoryValuations',
    'InventoryValuationLayers'
)
ORDER BY TABLE_NAME;
```

### Using EF Core CLI
```bash
dotnet ef database update --startup-project ../POSApi.Web.API --verbose
```

## Rollback (If Needed)

To rollback the migration:

```bash
# Get list of migrations
dotnet ef migrations list --startup-project ../POSApi.Web.API

# Rollback to previous migration (replace with actual previous migration name)
dotnet ef database update <PreviousMigrationName> --startup-project ../POSApi.Web.API

# Remove migration files
dotnet ef migrations remove --startup-project ../POSApi.Web.API
```

## Troubleshooting

### Issue: "Build failed"
**Solution**: Ensure the solution builds successfully before creating migration
```bash
cd ../POSApi.Web.API
dotnet build
```

### Issue: "Unable to connect to database"
**Solution**: Check connection string in appsettings.json

### Issue: "Migration already exists"
**Solution**: Remove existing migration first
```bash
dotnet ef migrations remove --startup-project ../POSApi.Web.API
```

### Issue: Foreign key constraint errors
**Solution**: Ensure all referenced tables exist. The migration should handle this automatically through proper ordering.

## Post-Migration Steps

1. **Verify Schema**: Check that all tables and columns were created correctly
2. **Test API Endpoints**: Verify all new API endpoints are working
3. **Seed Initial Data** (Optional): Add initial locations, alert configurations, etc.
4. **Update Documentation**: Update API documentation with new endpoints
5. **Deploy Frontend**: Ensure frontend is updated to match backend changes

## Seeding Sample Data (Optional)

After migration, you may want to seed some initial data:

```sql
-- Create default location
INSERT INTO Locations (Id, Name, Code, LocationType, IsActive, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Main Store', 'MAIN', 'Store', 1, GETDATE(), GETDATE());

-- Create warehouse location
INSERT INTO Locations (Id, Name, Code, LocationType, IsActive, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Main Warehouse', 'WH01', 'Warehouse', 1, GETDATE(), GETDATE());
```

## Production Deployment Checklist

- [ ] Backup production database
- [ ] Test migration in staging environment
- [ ] Review migration SQL scripts
- [ ] Schedule maintenance window
- [ ] Apply migration during low-traffic period
- [ ] Verify all tables created successfully
- [ ] Test critical API endpoints
- [ ] Monitor application logs for errors
- [ ] Rollback plan ready if needed

## Support

For issues or questions about the migration:
1. Check the error logs in `POSApi.Web.API/Logs/`
2. Review EF Core migration documentation
3. Consult the implementation guide at `IMPLEMENTATION_SUMMARY.md`
