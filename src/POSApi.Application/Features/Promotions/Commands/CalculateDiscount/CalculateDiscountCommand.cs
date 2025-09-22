using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Promotions.Commands.CalculateDiscount;

public class CalculateDiscountCommand : ICommand<DiscountCalculationResult>
{
    public string? CouponCode { get; set; }
    public decimal OrderAmount { get; set; }
    public List<OrderItemCalculation> OrderItems { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public bool ApplyAutomaticPromotions { get; set; } = true;
}

public class OrderItemCalculation
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class DiscountCalculationResult
{
    public decimal TotalDiscount { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<AppliedPromotion> AppliedPromotions { get; set; } = new();
    public bool HasErrors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}

public class AppliedPromotion
{
    public Guid PromotionId { get; set; }
    public string PromotionName { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CalculateDiscountCommandValidator : AbstractValidator<CalculateDiscountCommand>
{
    public CalculateDiscountCommandValidator()
    {
        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Order amount must be greater than 0");

        RuleFor(x => x.OrderItems)
            .NotEmpty().WithMessage("Order items are required");

        RuleForEach(x => x.OrderItems).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");
        });
    }
}