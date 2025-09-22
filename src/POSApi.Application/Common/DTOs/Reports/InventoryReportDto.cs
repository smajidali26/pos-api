namespace POSApi.Application.Common.DTOs.Reports;

public class InventoryReportDto
{
    public DateTime ReportDate { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<ProductInventoryDto> ProductInventory { get; set; } = new();
    public List<CategoryInventoryDto> CategoryBreakdown { get; set; } = new();
}

public class ProductInventoryDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string StockStatus { get; set; } = string.Empty; // "Normal", "Low", "Out of Stock"
}

public class CategoryInventoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalValue { get; set; }
}