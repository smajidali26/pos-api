namespace POSApi.Application.Common.DTOs.Analytics;

public class SalesAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }

    // Summary Metrics
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AvgOrderValue { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal TotalCost { get; set; }
    public int TotalItemsSold { get; set; }

    // Trends
    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<WeeklySalesDto> WeeklySales { get; set; } = new();
    public List<MonthlySalesDto> MonthlySales { get; set; } = new();

    // Top Performers
    public List<TopProductPerformanceDto> TopProducts { get; set; } = new();
    public List<TopCategoryPerformanceDto> TopCategories { get; set; } = new();
    public List<TopCustomerPerformanceDto> TopCustomers { get; set; } = new();

    // Sales by Time
    public List<HourlyPerformanceDto> SalesByHour { get; set; } = new();
    public List<DayOfWeekPerformanceDto> SalesByDayOfWeek { get; set; } = new();

    // Growth Metrics
    public decimal GrowthRate { get; set; }
    public decimal ComparisonPeriodSales { get; set; }
    public decimal PercentageChange { get; set; }
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Sales { get; set; }
    public int Orders { get; set; }
    public decimal Profit { get; set; }
    public decimal AvgOrderValue { get; set; }
}

public class WeeklySalesDto
{
    public int Year { get; set; }
    public int Week { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Sales { get; set; }
    public int Orders { get; set; }
    public decimal Profit { get; set; }
}

public class MonthlySalesDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public int Orders { get; set; }
    public decimal Profit { get; set; }
    public decimal GrowthRate { get; set; }
}

public class TopProductPerformanceDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int QuantitySold { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
    public int OrderCount { get; set; }
}

public class TopCategoryPerformanceDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int ItemsSold { get; set; }
    public decimal Profit { get; set; }
    public decimal ContributionPercentage { get; set; }
}

public class TopCustomerPerformanceDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public int OrderCount { get; set; }
    public decimal AvgOrderValue { get; set; }
    public DateTime LastPurchaseDate { get; set; }
}

public class HourlyPerformanceDto
{
    public int Hour { get; set; }
    public decimal AverageSales { get; set; }
    public int AverageOrders { get; set; }
}

public class DayOfWeekPerformanceDto
{
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public decimal AverageSales { get; set; }
    public int AverageOrders { get; set; }
}
