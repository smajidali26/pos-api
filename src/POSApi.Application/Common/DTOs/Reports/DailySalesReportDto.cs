using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Reports;

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