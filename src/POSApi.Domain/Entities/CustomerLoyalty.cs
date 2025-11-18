using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class CustomerLoyalty : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public int CurrentPoints { get; private set; }
    public int LifetimePoints { get; private set; }
    public decimal LifetimeSpend { get; private set; }
    public Guid? CurrentTierId { get; private set; }
    public CustomerTier? CurrentTier { get; private set; }
    public DateTime JoinDate { get; private set; }
    public DateTime LastActivityDate { get; private set; }
    public DateTime? LastPointsExpiryCheckDate { get; private set; }
    public ICollection<LoyaltyTransaction> Transactions { get; private set; } = new List<LoyaltyTransaction>();

    private CustomerLoyalty() { } // For EF Core

    public CustomerLoyalty(Guid customerId)
    {
        CustomerId = customerId;
        CurrentPoints = 0;
        LifetimePoints = 0;
        LifetimeSpend = 0;
        JoinDate = DateTime.UtcNow;
        LastActivityDate = DateTime.UtcNow;
    }

    public void EarnPoints(int points, string description, int expiryDays, Guid? orderId = null, decimal? orderAmount = null)
    {
        if (points <= 0)
        {
            throw new InvalidOperationException("Points to earn must be greater than zero");
        }

        var balanceBefore = CurrentPoints;
        CurrentPoints += points;
        LifetimePoints += points;

        if (orderAmount.HasValue)
        {
            LifetimeSpend += orderAmount.Value;
        }

        var transaction = new LoyaltyTransaction(
            Id,
            points,
            0,
            balanceBefore,
            CurrentPoints,
            description,
            LoyaltyTransactionType.Earned,
            orderId);

        transaction.SetExpiry(expiryDays);
        Transactions.Add(transaction);

        LastActivityDate = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void RedeemPoints(int points, string description, Guid? orderId = null)
    {
        if (points <= 0)
        {
            throw new InvalidOperationException("Points to redeem must be greater than zero");
        }

        if (CurrentPoints < points)
        {
            throw new InvalidOperationException($"Insufficient points. Available: {CurrentPoints}, Requested: {points}");
        }

        var balanceBefore = CurrentPoints;
        CurrentPoints -= points;

        var transaction = new LoyaltyTransaction(
            Id,
            0,
            points,
            balanceBefore,
            CurrentPoints,
            description,
            LoyaltyTransactionType.Redeemed,
            orderId);

        Transactions.Add(transaction);

        LastActivityDate = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public int ExpirePoints()
    {
        var expiredTransactions = Transactions
            .Where(t => t.TransactionType == LoyaltyTransactionType.Earned &&
                       t.IsExpired() &&
                       t.GetAvailablePoints() > 0)
            .ToList();

        int totalExpired = 0;

        foreach (var expiredTransaction in expiredTransactions)
        {
            var pointsToExpire = expiredTransaction.GetAvailablePoints();
            if (pointsToExpire > 0)
            {
                totalExpired += pointsToExpire;
            }
        }

        if (totalExpired > 0)
        {
            var balanceBefore = CurrentPoints;
            CurrentPoints -= totalExpired;

            var expiryTransaction = new LoyaltyTransaction(
                Id,
                0,
                totalExpired,
                balanceBefore,
                CurrentPoints,
                $"Points expired: {totalExpired} points",
                LoyaltyTransactionType.Expired);

            Transactions.Add(expiryTransaction);
        }

        LastPointsExpiryCheckDate = DateTime.UtcNow;
        SetUpdatedAt();

        return totalExpired;
    }

    public void AdjustPoints(int pointsAdjustment, string reason)
    {
        var balanceBefore = CurrentPoints;
        CurrentPoints += pointsAdjustment;

        if (CurrentPoints < 0)
        {
            CurrentPoints = 0;
        }

        // Only add to lifetime if positive adjustment
        if (pointsAdjustment > 0)
        {
            LifetimePoints += pointsAdjustment;
        }

        var transaction = new LoyaltyTransaction(
            Id,
            pointsAdjustment > 0 ? pointsAdjustment : 0,
            pointsAdjustment < 0 ? Math.Abs(pointsAdjustment) : 0,
            balanceBefore,
            CurrentPoints,
            reason,
            LoyaltyTransactionType.Adjusted);

        Transactions.Add(transaction);

        LastActivityDate = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void PromoteTier(Guid newTierId)
    {
        CurrentTierId = newTierId;
        SetUpdatedAt();
    }

    public decimal CalculatePointsValue(int pointsToRedeem, decimal pointValue = 0.01m)
    {
        return pointsToRedeem * pointValue;
    }

    public List<LoyaltyTransaction> GetExpiringTransactions(int daysThreshold)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);

        return Transactions
            .Where(t => t.TransactionType == LoyaltyTransactionType.Earned &&
                       t.ExpiryDate.HasValue &&
                       t.ExpiryDate.Value <= thresholdDate &&
                       t.ExpiryDate.Value > DateTime.UtcNow &&
                       t.GetAvailablePoints() > 0)
            .ToList();
    }
}
