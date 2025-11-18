using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class RewardRedemption : BaseEntity
{
    public Guid RewardId { get; private set; }
    public Reward Reward { get; private set; } = null!;
    public Guid CustomerLoyaltyId { get; private set; }
    public CustomerLoyalty CustomerLoyalty { get; private set; } = null!;
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public int PointsUsed { get; private set; }
    public DateTime RedeemedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime ExpiryDate { get; private set; }

    private RewardRedemption() { } // For EF Core

    public RewardRedemption(
        Guid rewardId,
        Guid customerLoyaltyId,
        int pointsUsed,
        int validityDays = 30)
    {
        RewardId = rewardId;
        CustomerLoyaltyId = customerLoyaltyId;
        PointsUsed = pointsUsed;
        RedeemedAt = DateTime.UtcNow;
        IsUsed = false;
        ExpiryDate = DateTime.UtcNow.AddDays(validityDays);
    }

    public void MarkAsUsed(Guid? orderId = null)
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Reward redemption has already been used");
        }

        if (CheckExpiry())
        {
            throw new InvalidOperationException("Reward redemption has expired");
        }

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        OrderId = orderId;
        SetUpdatedAt();
    }

    public bool CheckExpiry()
    {
        return DateTime.UtcNow > ExpiryDate;
    }

    public bool IsAvailable()
    {
        return !IsUsed && !CheckExpiry();
    }
}
