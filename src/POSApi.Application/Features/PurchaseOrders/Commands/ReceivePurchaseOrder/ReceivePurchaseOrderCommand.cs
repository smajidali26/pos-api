using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommand : ICommand
{
    public Guid PurchaseOrderId { get; set; }
    public Dictionary<Guid, int> ReceivedQuantities { get; set; } = new();
    public DateTime? ActualDeliveryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order ID is required");

        RuleFor(x => x.ReceivedQuantities)
            .NotEmpty().WithMessage("At least one item must be received");

        RuleForEach(x => x.ReceivedQuantities).ChildRules(item =>
        {
            item.RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage("Received quantity must be greater than 0");
        });

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}