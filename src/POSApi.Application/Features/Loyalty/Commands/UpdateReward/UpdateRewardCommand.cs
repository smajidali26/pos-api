using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Loyalty.Commands.UpdateReward;

public class UpdateRewardCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public RewardType RewardType { get; set; }
    public decimal Value { get; set; }
    public Guid? ProductId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxRedemptionsPerCustomer { get; set; }
    public int? TotalRedemptionsAllowed { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateRewardCommandValidator : AbstractValidator<UpdateRewardCommand>
{
    public UpdateRewardCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Reward ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Reward name is required")
            .MaximumLength(200).WithMessage("Reward name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.PointsCost)
            .GreaterThan(0).WithMessage("Points cost must be greater than 0");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Reward value must be greater than 0");

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Valid from date must be before valid to date");

        RuleFor(x => x.MaxRedemptionsPerCustomer)
            .GreaterThan(0).When(x => x.MaxRedemptionsPerCustomer.HasValue)
            .WithMessage("Max redemptions per customer must be greater than 0");

        RuleFor(x => x.TotalRedemptionsAllowed)
            .GreaterThan(0).When(x => x.TotalRedemptionsAllowed.HasValue)
            .WithMessage("Total redemptions allowed must be greater than 0");
    }
}
