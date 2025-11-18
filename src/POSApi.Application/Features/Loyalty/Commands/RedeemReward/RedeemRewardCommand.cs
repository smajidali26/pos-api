using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.RedeemReward;

public class RedeemRewardCommand : ICommand<Guid>
{
    public Guid CustomerId { get; set; }
    public Guid RewardId { get; set; }
}

public class RedeemRewardCommandValidator : AbstractValidator<RedeemRewardCommand>
{
    public RedeemRewardCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.RewardId)
            .NotEmpty().WithMessage("Reward ID is required");
    }
}
