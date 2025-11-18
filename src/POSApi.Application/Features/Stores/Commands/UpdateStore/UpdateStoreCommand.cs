using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Stores.Commands.UpdateStore;

public class UpdateStoreCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public StoreType? StoreType { get; set; }
    public Guid? ParentStoreId { get; set; }

    // Operating Hours
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public string? TimeZone { get; set; }

    // Financial Settings
    public decimal? TaxRate { get; set; }
    public string? Currency { get; set; }
}

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required")
            .MaximumLength(200).WithMessage("Store name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email address");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0m, 1m).When(x => x.TaxRate.HasValue)
            .WithMessage("Tax rate must be between 0 and 1");

        RuleFor(x => x.CloseTime)
            .GreaterThan(x => x.OpenTime).When(x => x.OpenTime.HasValue && x.CloseTime.HasValue)
            .WithMessage("Close time must be after open time");
    }
}
