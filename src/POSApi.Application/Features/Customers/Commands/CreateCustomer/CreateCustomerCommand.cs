using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommand : ICommand<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State must not exceed 50 characters");

        RuleFor(x => x.ZipCode)
            .MaximumLength(10).WithMessage("Zip code must not exceed 10 characters");
    }
}