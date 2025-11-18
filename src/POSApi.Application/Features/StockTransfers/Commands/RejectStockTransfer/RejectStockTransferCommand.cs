using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Commands.RejectStockTransfer;

public class RejectStockTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class RejectStockTransferCommandValidator : AbstractValidator<RejectStockTransferCommand>
{
    public RejectStockTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
