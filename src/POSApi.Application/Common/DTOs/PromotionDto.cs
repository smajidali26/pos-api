using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsStackable { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public string? CouponCode { get; set; }
    public PromotionTarget Target { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public List<PromotionProductDto> TargetProducts { get; set; } = new();
    public List<PromotionCategoryDto> TargetCategories { get; set; } = new();
    public List<PromotionUsageDto> Usages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties
    public bool IsCurrentlyActive { get; set; }
    public bool IsExpired { get; set; }
    public decimal PercentageUsed { get; set; }
}

public class PromotionProductDto
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
}

public class PromotionCategoryDto
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class PromotionUsageDto
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime UsedAt { get; set; }
}