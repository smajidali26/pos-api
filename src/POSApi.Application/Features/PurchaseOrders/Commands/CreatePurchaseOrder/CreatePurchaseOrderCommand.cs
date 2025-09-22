using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommand : ICommand<Guid>
{
    public Guid VendorId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseOrderItemRequest> Items { get; set; } = new();
}

public class PurchaseOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created By User ID is required");

        RuleFor(x => x.ExpectedDeliveryDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpectedDeliveryDate.HasValue)
            .WithMessage("Expected delivery date must be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitCost)
                .GreaterThan(0).WithMessage("Unit cost must be greater than 0");
        });
    }
}