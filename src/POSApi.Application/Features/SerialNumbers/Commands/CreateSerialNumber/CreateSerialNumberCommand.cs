using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.SerialNumbers.Commands.CreateSerialNumber;

public class CreateSerialNumberCommand : ICommand<Guid>
{
    public string Number { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? LocationId { get; set; }
    public int? WarrantyMonths { get; set; }
    public string? Notes { get; set; }
}

public class CreateSerialNumberCommandValidator : AbstractValidator<CreateSerialNumberCommand>
{
    public CreateSerialNumberCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Serial number is required")
            .MaximumLength(100).WithMessage("Serial number must not exceed 100 characters");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.WarrantyMonths)
            .GreaterThan(0).WithMessage("Warranty months must be greater than 0")
            .When(x => x.WarrantyMonths.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
