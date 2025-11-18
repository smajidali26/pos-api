using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class PromotionAnalyticsDto
{
    public int TotalPromotions { get; set; }
    public int ActivePromotions { get; set; }
    public int InactivePromotions { get; set; }
    public int ExpiredPromotions { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public int TotalUsageCount { get; set; }
    public decimal AverageDiscountPerPromotion { get; set; }
    public decimal AverageUsagePerPromotion { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<TopPromotionDto> TopPromotions { get; set; } = new();
    public List<PromotionTypeBreakdownDto> PromotionTypeBreakdown { get; set; } = new();
    public List<DiscountTypeBreakdownDto> DiscountTypeBreakdown { get; set; } = new();
}

public class TopPromotionDto
{
    public Guid PromotionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class PromotionTypeBreakdownDto
{
    public PromotionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalDiscountGiven { get; set; }
}

public class DiscountTypeBreakdownDto
{
    public DiscountType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalDiscountGiven { get; set; }
}
