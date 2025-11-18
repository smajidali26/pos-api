using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.AdjustPoints;

public class AdjustPointsCommand : ICommand
{
    public Guid CustomerId { get; set; }
    public int PointsAdjustment { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdjustPointsCommandValidator : AbstractValidator<AdjustPointsCommand>
{
    public AdjustPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.PointsAdjustment)
            .NotEqual(0).WithMessage("Points adjustment cannot be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
