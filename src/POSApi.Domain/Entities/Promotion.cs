using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Promotion : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PromotionType Type { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MinimumPurchaseAmount { get; private set; }
    public decimal? MaximumDiscountAmount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsStackable { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }
    public string? CouponCode { get; private set; }
    public PromotionTarget Target { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;

    // Navigation properties for targeted items
    public ICollection<PromotionProduct> PromotionProducts { get; private set; } = new List<PromotionProduct>();
    public ICollection<PromotionCategory> PromotionCategories { get; private set; } = new List<PromotionCategory>();
    public ICollection<PromotionUsage> PromotionUsages { get; private set; } = new List<PromotionUsage>();

    private Promotion() { } // For EF Core

    public Promotion(string name, string description, PromotionType type, DiscountType discountType, 
                    decimal discountValue, DateTime startDate, DateTime endDate, Guid createdByUserId,
                    PromotionTarget target = PromotionTarget.Order)
    {
        Name = name;
        Description = description;
        Type = type;
        DiscountType = discountType;
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        CreatedByUserId = createdByUserId;
        Target = target;
        IsActive = true;
        UsageCount = 0;

        ValidatePromotion();
        AddDomainEvent(new PromotionCreatedEvent(Id, name, type));
    }

    public void UpdateDetails(string name, string description, string notes = "")
    {
        Name = name;
        Description = description;
        Notes = notes;
        SetUpdatedAt();
    }

    public void UpdateDiscountSettings(DiscountType discountType, decimal discountValue, 
                                     decimal? minimumPurchase = null, decimal? maximumDiscount = null)
    {
        DiscountType = discountType;
        DiscountValue = discountValue;
        MinimumPurchaseAmount = minimumPurchase;
        MaximumDiscountAmount = maximumDiscount;
        
        ValidatePromotion();
        SetUpdatedAt();
    }

    public void UpdateDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new InvalidOperationException("End date must be after start date");
        }

        StartDate = startDate;
        EndDate = endDate;
        SetUpdatedAt();
    }

    public void SetCouponCode(string couponCode)
    {
        if (Type != PromotionType.Coupon)
        {
            throw new InvalidOperationException("Coupon code can only be set for coupon-type promotions");
        }

        CouponCode = couponCode;
        SetUpdatedAt();
    }

    public void SetUsageLimit(int usageLimit)
    {
        if (usageLimit < UsageCount)
        {
            throw new InvalidOperationException("Usage limit cannot be less than current usage count");
        }

        UsageLimit = usageLimit;
        SetUpdatedAt();
    }

    public void AddTargetProduct(Guid productId)
    {
        if (Target != PromotionTarget.Product && Target != PromotionTarget.ProductGroup)
        {
            throw new InvalidOperationException("Cannot add products to non-product targeted promotion");
        }

        var existing = PromotionProducts.FirstOrDefault(pp => pp.ProductId == productId);
        if (existing == null)
        {
            var promotionProduct = new PromotionProduct(Id, productId);
            PromotionProducts.Add(promotionProduct);
            SetUpdatedAt();
        }
    }

    public void AddTargetCategory(Guid categoryId)
    {
        if (Target != PromotionTarget.Category)
        {
            throw new InvalidOperationException("Cannot add categories to non-category targeted promotion");
        }

        var existing = PromotionCategories.FirstOrDefault(pc => pc.CategoryId == categoryId);
        if (existing == null)
        {
            var promotionCategory = new PromotionCategory(Id, categoryId);
            PromotionCategories.Add(promotionCategory);
            SetUpdatedAt();
        }
    }

    public void RecordUsage(Guid orderId, decimal discountAmount, Guid? customerId = null)
    {
        if (!IsValidForUse(DateTime.UtcNow))
        {
            throw new InvalidOperationException("Promotion is not valid for use");
        }

        if (UsageLimit.HasValue && UsageCount >= UsageLimit.Value)
        {
            throw new InvalidOperationException("Promotion usage limit has been reached");
        }

        var usage = new PromotionUsage(Id, orderId, discountAmount, customerId);
        PromotionUsages.Add(usage);
        UsageCount++;
        SetUpdatedAt();

        AddDomainEvent(new PromotionUsedEvent(Id, orderId, discountAmount, Name));
    }

    public decimal CalculateDiscount(decimal amount, int quantity = 1)
    {
        if (!IsValidForUse(DateTime.UtcNow))
        {
            return 0;
        }

        if (MinimumPurchaseAmount.HasValue && amount < MinimumPurchaseAmount.Value)
        {
            return 0;
        }

        decimal discount = DiscountType switch
        {
            DiscountType.Percentage => amount * (DiscountValue / 100),
            DiscountType.FixedAmount => DiscountValue,
            DiscountType.BuyXGetYFree => CalculateBuyXGetYDiscount(amount, quantity),
            _ => 0
        };

        if (MaximumDiscountAmount.HasValue && discount > MaximumDiscountAmount.Value)
        {
            discount = MaximumDiscountAmount.Value;
        }

        return Math.Max(0, Math.Min(discount, amount));
    }

    public bool IsValidForUse(DateTime checkDate)
    {
        return IsActive && 
               checkDate >= StartDate && 
               checkDate <= EndDate &&
               (!UsageLimit.HasValue || UsageCount < UsageLimit.Value);
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new PromotionActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new PromotionDeactivatedEvent(Id, Name));
    }

    private void ValidatePromotion()
    {
        if (DiscountValue <= 0)
        {
            throw new InvalidOperationException("Discount value must be greater than 0");
        }

        if (DiscountType == DiscountType.Percentage && DiscountValue > 100)
        {
            throw new InvalidOperationException("Percentage discount cannot exceed 100%");
        }

        if (EndDate <= StartDate)
        {
            throw new InvalidOperationException("End date must be after start date");
        }
    }

    private decimal CalculateBuyXGetYDiscount(decimal amount, int quantity)
    {
        // Simple implementation - could be more sophisticated
        int freeItems = quantity / (int)DiscountValue;
        return freeItems * (amount / quantity);
    }
}

// Junction entities for many-to-many relationships
public class PromotionProduct : BaseEntity
{
    public Guid PromotionId { get; private set; }
    public Promotion Promotion { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    private PromotionProduct() { } // For EF Core

    public PromotionProduct(Guid promotionId, Guid productId)
    {
        PromotionId = promotionId;
        ProductId = productId;
    }
}

public class PromotionCategory : BaseEntity
{
    public Guid PromotionId { get; private set; }
    public Promotion Promotion { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    private PromotionCategory() { } // For EF Core

    public PromotionCategory(Guid promotionId, Guid categoryId)
    {
        PromotionId = promotionId;
        CategoryId = categoryId;
    }
}

public class PromotionUsage : BaseEntity
{
    public Guid PromotionId { get; private set; }
    public Promotion Promotion { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public decimal DiscountAmount { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public DateTime UsedAt { get; private set; }

    private PromotionUsage() { } // For EF Core

    public PromotionUsage(Guid promotionId, Guid orderId, decimal discountAmount, Guid? customerId = null)
    {
        PromotionId = promotionId;
        OrderId = orderId;
        DiscountAmount = discountAmount;
        CustomerId = customerId;
        UsedAt = DateTime.UtcNow;
    }
}

public enum PromotionType
{
    General,        // Apply automatically
    Coupon,         // Requires coupon code
    LoyaltyReward,  // For loyalty program members
    StaffDiscount,  // For employees
    VolumeDiscount, // Based on quantity
    TimeBased      // Happy hour, etc.
}

public enum DiscountType
{
    Percentage,     // % off
    FixedAmount,    // $ off
    BuyXGetYFree   // Buy X get Y free
}

public enum PromotionTarget
{
    Order,          // Entire order
    Product,        // Specific products
    ProductGroup,   // Multiple specific products
    Category       // Product categories
}