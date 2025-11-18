namespace POSApi.Application.Common.DTOs.Analytics;

public class CustomerAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Summary Metrics
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int ReturnCustomers { get; set; }
    public decimal CustomerRetentionRate { get; set; }
    public decimal ChurnRate { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal AverageOrderValue { get; set; }

    // Customer Segmentation
    public List<CustomerSegmentDto> Segments { get; set; } = new();

    // Top Customers
    public List<TopCustomerDto> TopCustomersByRevenue { get; set; } = new();
    public List<TopCustomerDto> TopCustomersByFrequency { get; set; } = new();

    // Customer Behavior
    public decimal AveragePurchaseFrequency { get; set; }
    public decimal AverageTimeBetweenPurchases { get; set; }

    // RFM Analysis
    public List<RFMSegmentDto> RFMSegments { get; set; } = new();
}

public class CustomerSegmentDto
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class RFMSegmentDto
{
    public string Segment { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

public class CustomerLifetimeValueDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal LifetimeValue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime FirstPurchaseDate { get; set; }
    public DateTime LastPurchaseDate { get; set; }
    public int DaysSinceLastPurchase { get; set; }
    public string Segment { get; set; } = string.Empty;
}
