using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockAlerts.Commands.DismissAlert;

public class DismissAlertCommand : ICommand
{
    public Guid AlertId { get; set; }
    public string DismissReason { get; set; } = string.Empty;
}

public class DismissAlertCommandValidator : AbstractValidator<DismissAlertCommand>
{
    public DismissAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("Alert ID is required");

        RuleFor(x => x.DismissReason)
            .NotEmpty().WithMessage("Dismiss reason is required")
            .MaximumLength(500).WithMessage("Dismiss reason must not exceed 500 characters");
    }
}
