using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Vendors.Commands.UpdateVendor;

public class UpdateVendorCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public VendorType Type { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public decimal CreditLimit { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vendor name is required")
            .MaximumLength(200).WithMessage("Vendor name must not exceed 200 characters");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(300).WithMessage("Company name must not exceed 300 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(200).WithMessage("Contact person must not exceed 200 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State must not exceed 50 characters");

        RuleFor(x => x.ZipCode)
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters");

        RuleFor(x => x.Website)
            .MaximumLength(255).WithMessage("Website must not exceed 255 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be greater than or equal to 0");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters");
    }
}