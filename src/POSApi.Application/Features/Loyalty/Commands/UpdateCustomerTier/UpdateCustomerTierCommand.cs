using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.UpdateCustomerTier;

public class UpdateCustomerTierCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinSpend { get; set; }
    public int MinPoints { get; set; }
    public decimal BenefitMultiplier { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string Color { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateCustomerTierCommandValidator : AbstractValidator<UpdateCustomerTierCommand>
{
    public UpdateCustomerTierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Tier ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tier name is required")
            .MaximumLength(100).WithMessage("Tier name must not exceed 100 characters");

        RuleFor(x => x.MinSpend)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum spend must be greater than or equal to 0");

        RuleFor(x => x.MinPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum points must be greater than or equal to 0");

        RuleFor(x => x.BenefitMultiplier)
            .GreaterThan(0).WithMessage("Benefit multiplier must be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Benefit multiplier must not exceed 10");

        RuleFor(x => x.DiscountPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Discount percentage must be greater than or equal to 0")
            .LessThanOrEqualTo(100).WithMessage("Discount percentage must not exceed 100");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required")
            .MaximumLength(50).WithMessage("Color must not exceed 50 characters");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0");
    }
}
