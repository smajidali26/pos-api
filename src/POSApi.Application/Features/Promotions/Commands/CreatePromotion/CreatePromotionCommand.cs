using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Promotions.Commands.CreatePromotion;

public class CreatePromotionCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsStackable { get; set; }
    public int? UsageLimit { get; set; }
    public string? CouponCode { get; set; }
    public PromotionTarget Target { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public List<Guid> TargetProductIds { get; set; } = new();
    public List<Guid> TargetCategoryIds { get; set; } = new();
}

public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Promotion name is required")
            .MaximumLength(200).WithMessage("Promotion name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid promotion type is required");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Valid discount type is required");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("Percentage discount cannot exceed 100%")
            .When(x => x.DiscountType == DiscountType.Percentage);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.MinimumPurchaseAmount)
            .GreaterThan(0).WithMessage("Minimum purchase amount must be greater than 0")
            .When(x => x.MinimumPurchaseAmount.HasValue);

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThan(0).WithMessage("Maximum discount amount must be greater than 0")
            .When(x => x.MaximumDiscountAmount.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("Usage limit must be greater than 0")
            .When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required for coupon-type promotions")
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters")
            .When(x => x.Type == PromotionType.Coupon);

        RuleFor(x => x.Target)
            .IsInEnum().WithMessage("Valid promotion target is required");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.TargetProductIds)
            .NotEmpty().WithMessage("At least one product must be specified")
            .When(x => x.Target == PromotionTarget.Product || x.Target == PromotionTarget.ProductGroup);

        RuleFor(x => x.TargetCategoryIds)
            .NotEmpty().WithMessage("At least one category must be specified")
            .When(x => x.Target == PromotionTarget.Category);
    }
}