using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StoreInventory.Commands.AdjustStoreInventory;

public class AdjustStoreInventoryCommand : ICommand
{
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public int AdjustmentAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdjustStoreInventoryCommandValidator : AbstractValidator<AdjustStoreInventoryCommand>
{
    public AdjustStoreInventoryCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.AdjustmentAmount)
            .NotEqual(0).WithMessage("Adjustment amount cannot be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
