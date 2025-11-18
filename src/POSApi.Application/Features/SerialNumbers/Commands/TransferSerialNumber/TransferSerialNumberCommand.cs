using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.SerialNumbers.Commands.TransferSerialNumber;

public class TransferSerialNumberCommand : ICommand
{
    public Guid SerialNumberId { get; set; }
    public Guid ToLocationId { get; set; }
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
}

public class TransferSerialNumberCommandValidator : AbstractValidator<TransferSerialNumberCommand>
{
    public TransferSerialNumberCommandValidator()
    {
        RuleFor(x => x.SerialNumberId)
            .NotEmpty().WithMessage("Serial number ID is required");

        RuleFor(x => x.ToLocationId)
            .NotEmpty().WithMessage("Destination location ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
