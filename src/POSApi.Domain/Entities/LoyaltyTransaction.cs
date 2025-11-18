using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class LoyaltyTransaction : BaseEntity
{
    public Guid CustomerLoyaltyId { get; private set; }
    public CustomerLoyalty CustomerLoyalty { get; private set; } = null!;
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public int PointsEarned { get; private set; }
    public int PointsRedeemed { get; private set; }
    public int BalanceBefore { get; private set; }
    public int BalanceAfter { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public LoyaltyTransactionType TransactionType { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    private LoyaltyTransaction() { } // For EF Core

    public LoyaltyTransaction(
        Guid customerLoyaltyId,
        int pointsEarned,
        int pointsRedeemed,
        int balanceBefore,
        int balanceAfter,
        string description,
        LoyaltyTransactionType transactionType,
        Guid? orderId = null)
    {
        CustomerLoyaltyId = customerLoyaltyId;
        PointsEarned = pointsEarned;
        PointsRedeemed = pointsRedeemed;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        Description = description;
        TransactionType = transactionType;
        OrderId = orderId;
        TransactionDate = DateTime.UtcNow;
    }

    public void SetExpiry(int expiryDays)
    {
        if (PointsEarned > 0 && expiryDays > 0)
        {
            ExpiryDate = TransactionDate.AddDays(expiryDays);
        }
    }

    public bool IsExpired()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    public int GetAvailablePoints()
    {
        if (IsExpired() || TransactionType != LoyaltyTransactionType.Earned)
        {
            return 0;
        }

        return PointsEarned - PointsRedeemed;
    }
}

public enum LoyaltyTransactionType
{
    Earned,
    Redeemed,
    Expired,
    Adjusted,
    Bonus
}
