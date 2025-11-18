using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Stores.Commands.CreateStore;

public class CreateStoreCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid ManagerUserId { get; set; }
    public StoreType StoreType { get; set; } = StoreType.Branch;
    public Guid? ParentStoreId { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Operating Hours
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public string? TimeZone { get; set; }

    // Financial Settings
    public decimal? TaxRate { get; set; }
    public string? Currency { get; set; }
}

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required")
            .MaximumLength(200).WithMessage("Store name must not exceed 200 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Store code is required")
            .MaximumLength(50).WithMessage("Store code must not exceed 50 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.ManagerUserId)
            .NotEmpty().WithMessage("Manager is required");

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
