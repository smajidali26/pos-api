using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderCommand : ICommand
{
    public Guid PurchaseOrderId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}

public class CancelPurchaseOrderCommandValidator : AbstractValidator<CancelPurchaseOrderCommand>
{
    public CancelPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order ID is required");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("Cancellation reason is required")
            .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters");
    }
}