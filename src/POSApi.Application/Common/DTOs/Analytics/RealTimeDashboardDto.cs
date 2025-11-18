namespace POSApi.Application.Common.DTOs.Analytics;

public class RealTimeDashboardDto
{
    // Today's Metrics
    public decimal TodaySales { get; set; }
    public int TodayOrders { get; set; }
    public int TodayCustomers { get; set; }
    public decimal TodayAvgOrderValue { get; set; }
    public decimal TodayProfit { get; set; }
    public decimal TodayProfitMargin { get; set; }

    // Comparison Metrics
    public decimal YesterdaySales { get; set; }
    public decimal WoWChange { get; set; }
    public decimal MoMChange { get; set; }
    public decimal YoYChange { get; set; }

    // Hourly Breakdown
    public List<HourlySalesDto> HourlySales { get; set; } = new();

    // Top Performers
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<TopCategoryDto> TopCategories { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();

    // Payment Methods
    public List<PaymentMethodBreakdownDto> SalesByPaymentMethod { get; set; } = new();

    // Current Inventory Status
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public decimal TotalInventoryValue { get; set; }

    // Last Updated
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class HourlySalesDto
{
    public int Hour { get; set; }
    public decimal Sales { get; set; }
    public int Orders { get; set; }
    public decimal AvgOrderValue { get; set; }
}

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int QuantitySold { get; set; }
    public decimal Profit { get; set; }
}

public class TopCategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int ItemsSold { get; set; }
    public decimal ContributionPercentage { get; set; }
}

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public int OrderCount { get; set; }
    public decimal AvgOrderValue { get; set; }
}

public class PaymentMethodBreakdownDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}
