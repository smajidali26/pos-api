using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Commands.ApproveStockTransfer;

public class ApproveStockTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public Guid UserId { get; set; }
}

public class ApproveStockTransferCommandValidator : AbstractValidator<ApproveStockTransferCommand>
{
    public ApproveStockTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
