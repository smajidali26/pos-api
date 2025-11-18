using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Locations.Commands.ActivateLocation;

public class ActivateLocationCommand : ICommand
{
    public Guid LocationId { get; set; }
}

public class ActivateLocationCommandValidator : AbstractValidator<ActivateLocationCommand>
{
    public ActivateLocationCommandValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location ID is required");
    }
}
