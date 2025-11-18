using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.SerialNumbers.Commands.RepairSerialNumber;

public class RepairSerialNumberCommand : ICommand
{
    public Guid SerialNumberId { get; set; }
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
}

public class RepairSerialNumberCommandValidator : AbstractValidator<RepairSerialNumberCommand>
{
    public RepairSerialNumberCommandValidator()
    {
        RuleFor(x => x.SerialNumberId)
            .NotEmpty().WithMessage("Serial number ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
