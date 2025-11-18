using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Services;

namespace POSApi.Application.Features.Payments.Commands.ProcessRefund;

public class ProcessRefundCommand : ICommand<PaymentGatewayResult>
{
    public Guid PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ProcessRefundCommandValidator : AbstractValidator<ProcessRefundCommand>
{
    public ProcessRefundCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required");
    }
}
