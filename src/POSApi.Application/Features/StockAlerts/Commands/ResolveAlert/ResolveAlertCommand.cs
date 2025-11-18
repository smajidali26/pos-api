using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockAlerts.Commands.ResolveAlert;

public class ResolveAlertCommand : ICommand
{
    public Guid AlertId { get; set; }
    public Guid UserId { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class ResolveAlertCommandValidator : AbstractValidator<ResolveAlertCommand>
{
    public ResolveAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("Alert ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.ResolutionNotes)
            .MaximumLength(500).WithMessage("Resolution notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ResolutionNotes));
    }
}
