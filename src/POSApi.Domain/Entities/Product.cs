using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public string Barcode { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal Cost { get; private set; }
    public int StockQuantity { get; private set; }
    public int MinStockLevel { get; private set; }
    public int ReorderLevel { get; private set; }
    public int ReorderQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public Guid? PrimaryVendorId { get; private set; }
    public Vendor? PrimaryVendor { get; private set; }
    public string VendorProductCode { get; private set; } = string.Empty;
    public decimal LastPurchaseCost { get; private set; }
    public DateTime? LastPurchaseDate { get; private set; }

    // Unit of Measure Properties - now using database entities
    public ProductUnit? Unit { get; private set; }
    public decimal PricePerBaseUnit { get; private set; }
    public decimal CostPerBaseUnit { get; private set; }

    private Product() { } // For EF Core

    public Product(string name, string description, string sku, string barcode, 
                  decimal price, decimal cost, int stockQuantity, int minStockLevel, 
                  Guid categoryId, int reorderLevel = 0, int reorderQuantity = 0)
    {
        Name = name;
        Description = description;
        SKU = sku;
        Barcode = barcode;
        Price = price;
        Cost = cost;
        StockQuantity = stockQuantity;
        MinStockLevel = minStockLevel;
        ReorderLevel = reorderLevel > 0 ? reorderLevel : minStockLevel;
        ReorderQuantity = reorderQuantity;
        CategoryId = categoryId;

        AddDomainEvent(new ProductCreatedEvent(Id, name, price));
    }

    public void SetUnit(ProductUnit unit)
    {
        Unit = unit;
        RecalculatePerBaseUnitPricing();
        SetUpdatedAt();
    }

    public void UpdatePrice(decimal newPrice)
    {
        var oldPrice = Price;
        Price = newPrice;
        RecalculatePerBaseUnitPricing();
        SetUpdatedAt();
        
        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void UpdateCost(decimal newCost)
    {
        var oldCost = Cost;
        Cost = newCost;
        RecalculatePerBaseUnitPricing();
        SetUpdatedAt();
        
        AddDomainEvent(new ProductCostChangedEvent(Id, oldCost, newCost));
    }

    public void UpdateStock(int quantity)
    {
        var oldQuantity = StockQuantity;
        StockQuantity = quantity;
        SetUpdatedAt();

        if (StockQuantity <= MinStockLevel)
        {
            AddDomainEvent(new LowStockAlertEvent(Id, Name, StockQuantity, MinStockLevel));
        }

        if (StockQuantity <= ReorderLevel && ReorderQuantity > 0)
        {
            AddDomainEvent(new ReorderPointReachedEvent(Id, Name, StockQuantity, ReorderLevel, ReorderQuantity, PrimaryVendorId));
        }

        AddDomainEvent(new StockUpdatedEvent(Id, oldQuantity, StockQuantity));
    }

    public void ReduceStock(int quantity)
    {
        if (StockQuantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");
        }

        UpdateStock(StockQuantity - quantity);
    }

    public void IncreaseStock(int quantity)
    {
        UpdateStock(StockQuantity + quantity);
    }

    public void UpdateStockLevels(int minStockLevel, int reorderLevel, int reorderQuantity)
    {
        MinStockLevel = minStockLevel;
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        SetUpdatedAt();

        // Check if current stock is below new levels
        if (StockQuantity <= MinStockLevel)
        {
            AddDomainEvent(new LowStockAlertEvent(Id, Name, StockQuantity, MinStockLevel));
        }

        if (StockQuantity <= ReorderLevel && ReorderQuantity > 0)
        {
            AddDomainEvent(new ReorderPointReachedEvent(Id, Name, StockQuantity, ReorderLevel, ReorderQuantity, PrimaryVendorId));
        }
    }

    public void SetPrimaryVendor(Guid vendorId, string vendorProductCode = "")
    {
        var oldVendorId = PrimaryVendorId;
        PrimaryVendorId = vendorId;
        VendorProductCode = vendorProductCode;
        SetUpdatedAt();

        AddDomainEvent(new ProductVendorChangedEvent(Id, oldVendorId, vendorId));
    }

    public void RemovePrimaryVendor()
    {
        var oldVendorId = PrimaryVendorId;
        PrimaryVendorId = null;
        VendorProductCode = string.Empty;
        SetUpdatedAt();

        AddDomainEvent(new ProductVendorChangedEvent(Id, oldVendorId, null));
    }

    public void RecordPurchase(decimal cost, DateTime purchaseDate)
    {
        LastPurchaseCost = cost;
        LastPurchaseDate = purchaseDate;
        
        // Update cost if different
        if (Math.Abs(Cost - cost) > 0.01m)
        {
            UpdateCost(cost);
        }
        
        SetUpdatedAt();

        AddDomainEvent(new ProductPurchaseRecordedEvent(Id, cost, purchaseDate));
    }

    public void UpdateVendorProductCode(string vendorProductCode)
    {
        VendorProductCode = vendorProductCode;
        SetUpdatedAt();
    }

    public void UpdateBasicInfo(string name, string description)
    {
        Name = name;
        Description = description;
        SetUpdatedAt();
    }

    public void UpdateCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    private void RecalculatePerBaseUnitPricing()
    {
        if (Unit?.BaseUnit != null)
        {
            var quantityPerUnit = GetQuantityPerSellingUnit();
            PricePerBaseUnit = quantityPerUnit > 0 ? Price / quantityPerUnit : 0;
            CostPerBaseUnit = quantityPerUnit > 0 ? Cost / quantityPerUnit : 0;
        }
        else
        {
            PricePerBaseUnit = Price;
            CostPerBaseUnit = Cost;
        }
    }

    // UOM-related computed properties and methods
    
    /// <summary>
    /// Get the total quantity in base units
    /// </summary>
    public decimal GetTotalBaseUnitQuantity()
    {
        if (Unit?.BaseUnit == null) return StockQuantity;
        
        if (!Unit.HasPackaging)
            return StockQuantity * Unit.BaseQuantity;

        // With packaging: convert packaging to base units
        var baseUnitsPerPackage = Unit.PackagingUnit!.ConvertTo(Unit.PackagingQuantity!.Value, Unit.BaseUnit);
        return StockQuantity * baseUnitsPerPackage;
    }

    /// <summary>
    /// Get the selling unit (what customers buy)
    /// </summary>
    public UnitOfMeasure? GetSellingUnit()
    {
        return Unit?.HasPackaging == true ? Unit.PackagingUnit : Unit?.BaseUnit;
    }

    /// <summary>
    /// Get the quantity per selling unit in base units
    /// </summary>
    public decimal GetQuantityPerSellingUnit()
    {
        if (Unit?.BaseUnit == null) return 1m;
        
        if (!Unit.HasPackaging)
            return Unit.BaseQuantity;

        return Unit.PackagingUnit!.ConvertTo(Unit.PackagingQuantity!.Value, Unit.BaseUnit);
    }

    /// <summary>
    /// Calculate total value in inventory based on current stock
    /// </summary>
    public decimal GetInventoryValue()
    {
        return StockQuantity * Cost;
    }

    /// <summary>
    /// Get formatted display string for current stock
    /// </summary>
    public string GetStockDisplayString()
    {
        if (Unit?.BaseUnit == null)
            return $"{StockQuantity} units";

        var baseUnitQty = GetTotalBaseUnitQuantity();
        var sellingUnit = GetSellingUnit();
        
        return $"{StockQuantity} {Unit.DisplayName} ({baseUnitQty:F2} {Unit.BaseUnit.Symbol})";
    }

    /// <summary>
    /// Check if the product can be sold in a specific quantity
    /// </summary>
    public bool CanSell(decimal sellingUnitQuantity)
    {
        return StockQuantity >= sellingUnitQuantity && IsActive;
    }

    /// <summary>
    /// Calculate the selling price for a given quantity
    /// </summary>
    public decimal CalculateSellingPrice(decimal quantity)
    {
        return Price * quantity;
    }

    // Existing computed properties
    public bool NeedsReorder => StockQuantity <= ReorderLevel && ReorderQuantity > 0 && IsActive;
    public bool IsLowStock => StockQuantity <= MinStockLevel && IsActive;
    public bool IsOutOfStock => StockQuantity <= 0 && IsActive;
    
    // New UOM-aware computed properties
    public bool HasPhysicalWeight => Unit?.HasPhysicalWeight == true;
    public bool HasPhysicalVolume => Unit?.HasPhysicalVolume == true;
    public bool IsWeightBased => Unit?.BaseUnit?.UnitType?.Name == "Weight";
    public bool IsVolumeBased => Unit?.BaseUnit?.UnitType?.Name == "Volume";
    public bool IsCountBased => Unit?.BaseUnit?.UnitType?.Name == "Count";
}