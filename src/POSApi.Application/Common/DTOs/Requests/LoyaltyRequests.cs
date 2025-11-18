using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Requests;

public class CreateLoyaltyProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PointsPerDollar { get; set; }
    public decimal MinimumPurchaseAmount { get; set; }
    public int PointsExpiryDays { get; set; }
}

public class UpdateLoyaltyProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PointsPerDollar { get; set; }
    public decimal MinimumPurchaseAmount { get; set; }
    public int PointsExpiryDays { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTierRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal MinSpend { get; set; }
    public int MinPoints { get; set; }
    public decimal BenefitMultiplier { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string Color { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateTierRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal MinSpend { get; set; }
    public int MinPoints { get; set; }
    public decimal BenefitMultiplier { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string Color { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class EnrollCustomerRequest
{
    public Guid CustomerId { get; set; }
}

public class EarnPointsRequest
{
    public Guid CustomerId { get; set; }
    public decimal OrderAmount { get; set; }
    public Guid? OrderId { get; set; }
}

public class RedeemPointsRequest
{
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
}

public class AdjustPointsRequest
{
    public Guid CustomerId { get; set; }
    public int PointsAdjustment { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateRewardRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public RewardType RewardType { get; set; }
    public decimal Value { get; set; }
    public Guid? ProductId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxRedemptionsPerCustomer { get; set; }
    public int? TotalRedemptionsAllowed { get; set; }
}

public class UpdateRewardRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public RewardType RewardType { get; set; }
    public decimal Value { get; set; }
    public Guid? ProductId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxRedemptionsPerCustomer { get; set; }
    public int? TotalRedemptionsAllowed { get; set; }
    public bool IsActive { get; set; }
}

public class RedeemRewardRequest
{
    public Guid CustomerId { get; set; }
    public Guid RewardId { get; set; }
}
