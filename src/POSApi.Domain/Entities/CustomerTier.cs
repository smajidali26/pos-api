using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class CustomerTier : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public decimal MinSpend { get; private set; }
    public int MinPoints { get; private set; }
    public decimal BenefitMultiplier { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private CustomerTier() { } // For EF Core

    public CustomerTier(
        string name,
        decimal minSpend,
        int minPoints,
        decimal benefitMultiplier,
        decimal discountPercentage,
        string color,
        int sortOrder)
    {
        Name = name;
        MinSpend = minSpend;
        MinPoints = minPoints;
        BenefitMultiplier = benefitMultiplier;
        DiscountPercentage = discountPercentage;
        Color = color;
        SortOrder = sortOrder;
    }

    public void Update(
        string name,
        decimal minSpend,
        int minPoints,
        decimal benefitMultiplier,
        decimal discountPercentage,
        string color,
        int sortOrder)
    {
        Name = name;
        MinSpend = minSpend;
        MinPoints = minPoints;
        BenefitMultiplier = benefitMultiplier;
        DiscountPercentage = discountPercentage;
        Color = color;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }

    public bool CheckEligibility(int lifetimePoints, decimal lifetimeSpend)
    {
        return lifetimePoints >= MinPoints || lifetimeSpend >= MinSpend;
    }

    public decimal ApplyBenefits(decimal orderAmount)
    {
        return orderAmount * (DiscountPercentage / 100);
    }

    public int ApplyPointsMultiplier(int basePoints)
    {
        return (int)Math.Floor(basePoints * BenefitMultiplier);
    }
}
