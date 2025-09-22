using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.SubmitPurchaseOrder;

public class SubmitPurchaseOrderCommand : ICommand
{
    public Guid PurchaseOrderId { get; set; }
}

public class SubmitPurchaseOrderCommandValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
    public SubmitPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order ID is required");
    }
}