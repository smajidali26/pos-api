# Fix: "Invalid column name Size" Error

## Problem

The application was throwing the following error when querying orders:

```
"Invalid column name Size"
```

**Location**: `OrderRepository.cs:150`

## Root Cause

The `Product` entity in the domain model had a `Size` property (line 10 in `Product.cs`):

```csharp
public string Size { get; private set; } = string.Empty; // e.g., "1L", "500ml", "XL", "1KG"
```

However, this property was **not configured** in `ProductConfiguration.cs`, and the `Size` column **did not exist** in the database.

When Entity Framework tried to query `Product` entities (e.g., when loading OrderItems with their associated Products), it attempted to map the `Size` property to a database column that didn't exist, causing the SQL error.

## Solution Applied

### 1. Updated Entity Configuration

**File**: `D:\src\POS\pos-api\src\POSApi.Infrastructure\Persistence\Configurations\ProductConfiguration.cs`

Added the `Size` property configuration:

```csharp
builder.Property(p => p.Size)
    .HasMaxLength(50);
```

This tells Entity Framework to map the `Size` property to a database column with `nvarchar(50)` type.

### 2. Created Database Migration

Generated an Entity Framework migration to add the `Size` column:

```bash
dotnet ef migrations add AddSizeColumnToProduct
```

**Migration File**: `20251109184918_AddSizeColumnToProduct.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "Size",
        table: "Product",
        type: "nvarchar(50)",
        maxLength: 50,
        nullable: false,
        defaultValue: "");
}
```

### 3. Applied Database Changes

Since the database had existing data and pending migrations couldn't be applied directly, we:

1. Generated an idempotent SQL script from the migration
2. Executed the SQL script directly against the database

```sql
ALTER TABLE [Product] ADD [Size] nvarchar(50) NOT NULL DEFAULT N'';
```

### 4. Verified the Fix

**Database Verification**:
```sql
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'Size'
```

**Result**:
- Column Name: `Size`
- Data Type: `nvarchar`
- Max Length: `50`
- Nullable: `NO` (with default value '')

**Build Verification**:
```bash
dotnet build POSApi.sln
```

**Result**: Build succeeded with 0 warnings and 0 errors

## Testing

The error should now be resolved. To test:

1. **Start the backend**:
   ```bash
   cd D:\src\POS\pos-api\src\POSApi.Web.API
   dotnet run
   ```

2. **Test order queries** that include products (e.g., GET /api/orders)

3. **Verify** that OrderRepository queries work without the "Invalid column name Size" error

## Files Modified

1. **ProductConfiguration.cs** - Added Size property configuration
2. **20251109184918_AddSizeColumnToProduct.cs** - Migration file (created)
3. **Database** - `Product` table now has `Size` column

## Prevention

To prevent similar issues in the future:

1. **Always configure entity properties** in the corresponding `IEntityTypeConfiguration` class
2. **Create migrations** after adding new properties to entities
3. **Test queries** after schema changes to catch missing columns early
4. **Review EF Core conventions** - by default, EF tries to map all public properties unless explicitly ignored

## Additional Notes

### Why Size Property Exists

The `Size` property is used to store product sizing information like:
- Liquids: "1L", "500ml", "250ml"
- Clothing: "XL", "L", "M", "S"
- Weight-based: "1KG", "500g"

This complements the Unit of Measure (UOM) system but provides a simple string field for display purposes.

### Default Value

Existing products in the database now have an empty string (`''`) as the default value for the `Size` field. You may want to:

1. Update existing products with appropriate size values
2. Or leave them empty if size information isn't relevant for those products

## Related Files

- `Product.cs` - Domain entity with Size property
- `ProductConfiguration.cs` - EF Core configuration
- `OrderRepository.cs` - Where the error originally occurred
- `__EFMigrationsHistory` table - Migration tracking in database
