using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.EarnPoints;

public class EarnPointsCommand : ICommand<int>
{
    public Guid CustomerId { get; set; }
    public decimal OrderAmount { get; set; }
    public Guid? OrderId { get; set; }
}

public class EarnPointsCommandValidator : AbstractValidator<EarnPointsCommand>
{
    public EarnPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Order amount must be greater than 0");
    }
}
