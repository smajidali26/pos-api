namespace POSApi.Application.Common.DTOs.Reports;

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