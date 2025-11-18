using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class Reward : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int PointsCost { get; private set; }
    public RewardType RewardType { get; private set; }
    public decimal Value { get; private set; }
    public Guid? ProductId { get; private set; }
    public Product? Product { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public int? MaxRedemptionsPerCustomer { get; private set; }
    public int? TotalRedemptionsAllowed { get; private set; }
    public int CurrentRedemptions { get; private set; }
    public bool IsActive { get; private set; }

    private Reward() { } // For EF Core

    public Reward(
        string name,
        string description,
        int pointsCost,
        RewardType rewardType,
        decimal value,
        DateTime validFrom,
        DateTime validTo,
        Guid? productId = null,
        int? maxRedemptionsPerCustomer = null,
        int? totalRedemptionsAllowed = null)
    {
        Name = name;
        Description = description;
        PointsCost = pointsCost;
        RewardType = rewardType;
        Value = value;
        ProductId = productId;
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxRedemptionsPerCustomer = maxRedemptionsPerCustomer;
        TotalRedemptionsAllowed = totalRedemptionsAllowed;
        CurrentRedemptions = 0;
        IsActive = true;
    }

    public void Update(
        string name,
        string description,
        int pointsCost,
        RewardType rewardType,
        decimal value,
        DateTime validFrom,
        DateTime validTo,
        Guid? productId = null,
        int? maxRedemptionsPerCustomer = null,
        int? totalRedemptionsAllowed = null)
    {
        Name = name;
        Description = description;
        PointsCost = pointsCost;
        RewardType = rewardType;
        Value = value;
        ProductId = productId;
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxRedemptionsPerCustomer = maxRedemptionsPerCustomer;
        TotalRedemptionsAllowed = totalRedemptionsAllowed;
        SetUpdatedAt();
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

    public bool CheckValidity()
    {
        var now = DateTime.UtcNow;
        return IsActive && now >= ValidFrom && now <= ValidTo;
    }

    public bool IsAvailableForRedemption(int customerRedemptions)
    {
        if (!CheckValidity())
        {
            return false;
        }

        // Check per-customer limit
        if (MaxRedemptionsPerCustomer.HasValue && customerRedemptions >= MaxRedemptionsPerCustomer.Value)
        {
            return false;
        }

        // Check total redemptions limit
        if (TotalRedemptionsAllowed.HasValue && CurrentRedemptions >= TotalRedemptionsAllowed.Value)
        {
            return false;
        }

        return true;
    }

    public void Redeem()
    {
        if (!CheckValidity())
        {
            throw new InvalidOperationException("Reward is not currently valid");
        }

        if (TotalRedemptionsAllowed.HasValue && CurrentRedemptions >= TotalRedemptionsAllowed.Value)
        {
            throw new InvalidOperationException("Reward redemption limit reached");
        }

        CurrentRedemptions++;
        SetUpdatedAt();
    }
}

public enum RewardType
{
    Discount,
    FreeProduct,
    Cashback,
    ServiceVoucher
}
