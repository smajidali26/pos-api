using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public class SendPurchaseOrderCommand : ICommand
{
    public Guid PurchaseOrderId { get; set; }
}

public class SendPurchaseOrderCommandValidator : AbstractValidator<SendPurchaseOrderCommand>
{
    public SendPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order ID is required");
    }
}