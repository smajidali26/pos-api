using FluentValidation;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.ConvertUnits;

public class ConvertUnitsCommand : ICommand<UnitConversionResultDto>
{
    public decimal Quantity { get; set; }
    public string FromUnitCode { get; set; } = string.Empty;
    public string ToUnitCode { get; set; } = string.Empty;
}

public class ConvertUnitsCommandValidator : AbstractValidator<ConvertUnitsCommand>
{
    public ConvertUnitsCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.FromUnitCode)
            .NotEmpty().WithMessage("From unit code is required");

        RuleFor(x => x.ToUnitCode)
            .NotEmpty().WithMessage("To unit code is required");
    }
}
