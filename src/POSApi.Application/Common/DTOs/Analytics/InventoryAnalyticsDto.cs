namespace POSApi.Application.Common.DTOs.Analytics;

public class InventoryAnalyticsDto
{
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }

    // Summary Metrics
    public decimal TotalInventoryValue { get; set; }
    public decimal AverageTurnoverRate { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int OverstockedProducts { get; set; }
    public int DeadStockProducts { get; set; }

    // Inventory Health
    public decimal HealthScore { get; set; }
    public decimal CarryingCost { get; set; }
    public decimal ObsolescenceRisk { get; set; }

    // By Category
    public List<CategoryInventoryDto> ByCategory { get; set; } = new();

    // By Store (if multi-store analysis)
    public List<StoreInventoryDto> ByStore { get; set; } = new();

    // Reorder Recommendations
    public List<ReorderRecommendationDto> ReorderRecommendations { get; set; } = new();

    // Overstocked Items
    public List<OverstockedItemDto> OverstockedItems { get; set; } = new();

    // Dead Stock
    public List<DeadStockItemDto> DeadStockItems { get; set; } = new();

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class CategoryInventoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal InventoryValue { get; set; }
    public int ProductCount { get; set; }
    public decimal AverageTurnover { get; set; }
    public int LowStockCount { get; set; }
}

public class StoreInventoryDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal InventoryValue { get; set; }
    public int ProductCount { get; set; }
    public decimal TurnoverRate { get; set; }
}

public class ReorderRecommendationDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int RecommendedOrderQuantity { get; set; }
    public decimal AverageDailySales { get; set; }
    public int DaysUntilStockout { get; set; }
    public string Priority { get; set; } = string.Empty;
    public Guid? PreferredVendorId { get; set; }
    public string? VendorName { get; set; }
}

public class OverstockedItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal AverageDailySales { get; set; }
    public decimal MonthsOfSupply { get; set; }
    public decimal ExcessQuantity { get; set; }
    public decimal TiedUpCapital { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class DeadStockItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public int DaysSinceLastSale { get; set; }
    public decimal InventoryValue { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}
