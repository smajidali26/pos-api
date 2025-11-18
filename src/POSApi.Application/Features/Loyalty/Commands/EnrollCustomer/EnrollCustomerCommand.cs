using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.EnrollCustomer;

public class EnrollCustomerCommand : ICommand<Guid>
{
    public Guid CustomerId { get; set; }
}

public class EnrollCustomerCommandValidator : AbstractValidator<EnrollCustomerCommand>
{
    public EnrollCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}
