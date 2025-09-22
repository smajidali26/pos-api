using POSApi.Domain.Entities;
using POSApi.Infrastructure.DTOs.Reports;

namespace POSApi.Infrastructure.DTOs.Reports;

public class DailySalesReportDto
{
    public DateTime ReportDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal NetSales { get; set; }
    public List<PaymentMethodSummary> PaymentMethodBreakdown { get; set; } = new();
    public List<HourlySales> HourlySalesBreakdown { get; set; } = new();
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<CashierPerformanceDto> CashierPerformance { get; set; } = new();
}

public class PaymentMethodSummary
{
    public PaymentMethod PaymentMethod { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

public class HourlySales
{
    public int Hour { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
}

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class CashierPerformanceDto
{
    public Guid CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public int OrdersProcessed { get; set; }
    public decimal TotalSales { get; set; }
    public decimal AverageOrderValue { get; set; }
}

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
    public string StockStatus { get; set; } = string.Empty;
}

public class CategoryInventoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalValue { get; set; }
}

public class CustomerAnalyticsReportDto
{
    public DateTime ReportDate { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersThisPeriod { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal CustomerLifetimeValue { get; set; }
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    public List<CustomerSegmentDto> CustomerSegments { get; set; } = new();
    public LoyaltyProgramSummary LoyaltyProgram { get; set; } = new();
}

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public DateTime LastPurchase { get; set; }
}

public class CustomerSegmentDto
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class LoyaltyProgramSummary
{
    public int TotalLoyaltyMembers { get; set; }
    public decimal TotalPointsIssued { get; set; }
    public decimal TotalPointsRedeemed { get; set; }
    public decimal PointsOutstanding { get; set; }
    public decimal AveragePointsPerCustomer { get; set; }
}