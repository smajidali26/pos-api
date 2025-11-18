using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Services;

namespace POSApi.Application.Features.Payments.Commands.CreatePaymentIntent;

public class CreatePaymentIntentCommand : ICommand<PaymentIntentResult>
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public Guid OrderId { get; set; }
}

public class CreatePaymentIntentCommandValidator : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}
