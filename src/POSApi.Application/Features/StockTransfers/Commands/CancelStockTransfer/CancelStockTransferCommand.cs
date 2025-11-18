using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Commands.CancelStockTransfer;

public class CancelStockTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class CancelStockTransferCommandValidator : AbstractValidator<CancelStockTransferCommand>
{
    public CancelStockTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("Cancellation reason is required")
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
