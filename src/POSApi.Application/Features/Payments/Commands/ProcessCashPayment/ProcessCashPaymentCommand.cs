using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Payments.Commands.ProcessCashPayment;

public class ProcessCashPaymentCommand : ICommand<Payment>
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}

public class ProcessCashPaymentCommandValidator : AbstractValidator<ProcessCashPaymentCommand>
{
    public ProcessCashPaymentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}
