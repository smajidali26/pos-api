using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Unit of Measure Properties
    public ProductUnitDto Unit { get; set; } = new();
    public decimal PricePerBaseUnit { get; set; }
    public decimal CostPerBaseUnit { get; set; }
    public decimal TotalBaseUnitQuantity { get; set; }
    public string StockDisplayString { get; set; } = string.Empty;

    // Physical Properties
    public decimal? Weight { get; set; }
    public string? WeightUnitSymbol { get; set; }
    public decimal? Volume { get; set; }
    public string? VolumeUnitSymbol { get; set; }

    // Computed Properties
    public decimal InventoryValue { get; set; }
    public bool NeedsReorder { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsWeightBased { get; set; }
    public bool IsVolumeBased { get; set; }
    public bool IsCountBased { get; set; }

    // Vendor Information
    public Guid? PrimaryVendorId { get; set; }
    public string? PrimaryVendorName { get; set; }
    public string VendorProductCode { get; set; } = string.Empty;
    public decimal LastPurchaseCost { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
}

public class ProductUnitDto
{
    public UnitOfMeasureDto BaseUnit { get; set; } = new();
    public decimal BaseQuantity { get; set; } = 1m;
    public UnitOfMeasureDto? PackagingUnit { get; set; }
    public decimal? PackagingQuantity { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool HasPackaging { get; set; }
    public UnitOfMeasureDto SellingUnit { get; set; } = new();
    public decimal QuantityPerSellingUnit { get; set; }

    // Physical Properties
    public decimal? Weight { get; set; }
    public UnitOfMeasureDto? WeightUnit { get; set; }
    public decimal? Volume { get; set; }
    public UnitOfMeasureDto? VolumeUnit { get; set; }
    public bool HasPhysicalWeight { get; set; }
    public bool HasPhysicalVolume { get; set; }
}

public class UnitOfMeasureDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal ConversionFactorToBase { get; set; } = 1m;
    public bool IsBaseUnit { get; set; }
}

// Pagination DTOs
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

public class ProductSearchRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLowStock { get; set; }
    public bool IncludeInactive { get; set; } = false;
}