namespace POSApi.Application.Common.DTOs.Loyalty;

public class LoyaltyDashboardDto
{
    public int TotalActiveMembers { get; set; }
    public int NewMembersThisMonth { get; set; }
    public int TotalPointsIssued { get; set; }
    public int TotalPointsRedeemed { get; set; }
    public int PointsOutstanding { get; set; }
    public decimal TotalLifetimeSpend { get; set; }
    public decimal AveragePointsPerMember { get; set; }
    public decimal RedemptionRate { get; set; }
    public List<TierDistributionDto> TierDistribution { get; set; } = new();
    public List<TopLoyaltyCustomerDto> TopCustomers { get; set; } = new();
    public List<MonthlyPointsActivityDto> MonthlyActivity { get; set; } = new();
}

public class TierDistributionDto
{
    public Guid TierId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public decimal Percentage { get; set; }
}

public class TopLoyaltyCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public int LifetimePoints { get; set; }
    public decimal LifetimeSpend { get; set; }
    public string TierName { get; set; } = string.Empty;
}

public class MonthlyPointsActivityDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public int NetPoints { get; set; }
}
