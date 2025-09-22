using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommand : ICommand
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ApprovedByUserId { get; set; }
}

public class ApprovePurchaseOrderCommandValidator : AbstractValidator<ApprovePurchaseOrderCommand>
{
    public ApprovePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order ID is required");

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty().WithMessage("Approved By User ID is required");
    }
}