namespace POSApi.Application.Common.DTOs.Analytics;

public class InventoryTurnoverDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public decimal TurnoverRatio { get; set; }
    public decimal DaysToSell { get; set; }
    public decimal AverageCOGS { get; set; }
    public decimal AverageInventoryValue { get; set; }
    public string Classification { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal EstimatedMonthsOfSupply { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class InventoryTurnoverSummaryDto
{
    public decimal OverallTurnoverRatio { get; set; }
    public decimal AverageDaysToSell { get; set; }
    public int FastMovingItems { get; set; }
    public int NormalMovingItems { get; set; }
    public int SlowMovingItems { get; set; }
    public int DeadStockItems { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<InventoryTurnoverDto> Items { get; set; } = new();
    public List<CategoryTurnoverDto> ByCategory { get; set; } = new();
}

public class CategoryTurnoverDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal AverageTurnoverRatio { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int ProductCount { get; set; }
}
