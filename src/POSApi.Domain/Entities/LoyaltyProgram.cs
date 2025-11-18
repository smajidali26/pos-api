using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class LoyaltyProgram : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal PointsPerDollar { get; private set; }
    public decimal MinimumPurchaseAmount { get; private set; }
    public int PointsExpiryDays { get; private set; }
    public bool IsActive { get; private set; }

    private LoyaltyProgram() { } // For EF Core

    public LoyaltyProgram(
        string name,
        string description,
        decimal pointsPerDollar,
        decimal minimumPurchaseAmount,
        int pointsExpiryDays)
    {
        Name = name;
        Description = description;
        PointsPerDollar = pointsPerDollar;
        MinimumPurchaseAmount = minimumPurchaseAmount;
        PointsExpiryDays = pointsExpiryDays;
        IsActive = false; // Needs to be explicitly activated
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void UpdateRules(decimal pointsPerDollar, int pointsExpiryDays)
    {
        PointsPerDollar = pointsPerDollar;
        PointsExpiryDays = pointsExpiryDays;
        SetUpdatedAt();
    }

    public void UpdateDetails(string name, string description, decimal minimumPurchaseAmount)
    {
        Name = name;
        Description = description;
        MinimumPurchaseAmount = minimumPurchaseAmount;
        SetUpdatedAt();
    }

    public int CalculatePoints(decimal orderAmount)
    {
        if (orderAmount < MinimumPurchaseAmount)
        {
            return 0;
        }

        return (int)Math.Floor(orderAmount * PointsPerDollar);
    }
}
