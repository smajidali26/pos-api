using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.CreateLoyaltyProgram;

public class CreateLoyaltyProgramCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PointsPerDollar { get; set; }
    public decimal MinimumPurchaseAmount { get; set; }
    public int PointsExpiryDays { get; set; }
}

public class CreateLoyaltyProgramCommandValidator : AbstractValidator<CreateLoyaltyProgramCommand>
{
    public CreateLoyaltyProgramCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Program name is required")
            .MaximumLength(200).WithMessage("Program name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.PointsPerDollar)
            .GreaterThan(0).WithMessage("Points per dollar must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Points per dollar must not exceed 100");

        RuleFor(x => x.MinimumPurchaseAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum purchase amount must be greater than or equal to 0");

        RuleFor(x => x.PointsExpiryDays)
            .GreaterThan(0).WithMessage("Points expiry days must be greater than 0")
            .LessThanOrEqualTo(3650).WithMessage("Points expiry days must not exceed 3650 (10 years)");
    }
}
