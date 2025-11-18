using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.RedeemPoints;

public class RedeemPointsCommand : ICommand
{
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
}

public class RedeemPointsCommandValidator : AbstractValidator<RedeemPointsCommand>
{
    public RedeemPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Points to redeem must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
