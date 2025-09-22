using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Promotions.Commands.ValidateCoupon;

public class ValidateCouponCommand : ICommand<CouponValidationResult>
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
    public Guid? CustomerId { get; set; }
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? PromotionId { get; set; }
    public decimal PotentialDiscount { get; set; }
    public string PromotionName { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
}

public class ValidateCouponCommandValidator : AbstractValidator<ValidateCouponCommand>
{
    public ValidateCouponCommandValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required");

        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Order amount must be greater than 0");
    }
}