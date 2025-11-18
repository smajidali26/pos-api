namespace POSApi.Application.Common.DTOs.Loyalty;

public class RewardRedemptionDto
{
    public Guid Id { get; set; }
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardDescription { get; set; } = string.Empty;
    public Guid CustomerLoyaltyId { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int PointsUsed { get; set; }
    public DateTime RedeemedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
}
