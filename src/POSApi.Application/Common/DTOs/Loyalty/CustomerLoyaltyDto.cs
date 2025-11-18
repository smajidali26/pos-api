namespace POSApi.Application.Common.DTOs.Loyalty;

public class CustomerLoyaltyDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public int LifetimePoints { get; set; }
    public decimal LifetimeSpend { get; set; }
    public Guid? CurrentTierId { get; set; }
    public CustomerTierDto? CurrentTier { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime LastActivityDate { get; set; }
    public DateTime? LastPointsExpiryCheckDate { get; set; }
    public int PointsExpiringIn30Days { get; set; }
}
