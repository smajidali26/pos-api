using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Locations.Commands.DeactivateLocation;

public class DeactivateLocationCommand : ICommand
{
    public Guid LocationId { get; set; }
}

public class DeactivateLocationCommandValidator : AbstractValidator<DeactivateLocationCommand>
{
    public DeactivateLocationCommandValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location ID is required");
    }
}
