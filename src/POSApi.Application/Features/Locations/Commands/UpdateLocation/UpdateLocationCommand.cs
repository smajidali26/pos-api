using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Locations.Commands.UpdateLocation;

public class UpdateLocationCommand : ICommand
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ParentLocationId { get; set; }
}

public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Location name is required")
            .MaximumLength(100).WithMessage("Location name must not exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Location code is required")
            .MaximumLength(20).WithMessage("Location code must not exceed 20 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Location code must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.LocationType)
            .IsInEnum().WithMessage("Invalid location type");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ParentLocationId)
            .NotEqual(x => x.LocationId).WithMessage("Location cannot be its own parent")
            .When(x => x.ParentLocationId.HasValue);
    }
}
