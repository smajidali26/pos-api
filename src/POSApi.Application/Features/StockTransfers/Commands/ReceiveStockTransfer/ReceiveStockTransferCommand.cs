using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Commands.ReceiveStockTransfer;

public class ReceiveStockTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public int ReceivedQuantity { get; set; }
    public string? ReceiverNotes { get; set; }
    public Guid UserId { get; set; }
}

public class ReceiveStockTransferCommandValidator : AbstractValidator<ReceiveStockTransferCommand>
{
    public ReceiveStockTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.ReceivedQuantity)
            .GreaterThan(0).WithMessage("Received quantity must be greater than 0");

        RuleFor(x => x.ReceiverNotes)
            .MaximumLength(500).WithMessage("Receiver notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ReceiverNotes));

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
