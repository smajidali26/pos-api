using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;

public class CompletePurchaseOrderCommand : ICommand
{
    public Guid PurchaseOrderId { get; set; }
    public string CompletionNotes { get; set; } = string.Empty;
}

public class CompletePurchaseOrderCommandValidator : AbstractValidator<CompletePurchaseOrderCommand>
{
    public CompletePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase Order ID is required");

        RuleFor(x => x.CompletionNotes)
            .MaximumLength(2000).WithMessage("Completion notes cannot exceed 2000 characters");
    }
}