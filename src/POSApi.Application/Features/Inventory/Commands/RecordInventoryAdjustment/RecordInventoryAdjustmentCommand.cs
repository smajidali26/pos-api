using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Inventory.Commands.RecordInventoryAdjustment;

public class RecordInventoryAdjustmentCommand : ICommand<Guid>
{
    public Guid ProductId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public Guid? ReferenceId { get; set; }
    public decimal? UnitCost { get; set; }
    public Guid? LocationId { get; set; }
    public Guid UserId { get; set; }  // Set from controller using HttpContext
}

public class RecordInventoryAdjustmentCommandValidator : AbstractValidator<RecordInventoryAdjustmentCommand>
{
    public RecordInventoryAdjustmentCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("New quantity must be greater than or equal to 0");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required for inventory adjustment")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");

        RuleFor(x => x.UnitCost)
            .GreaterThan(0).WithMessage("Unit cost must be greater than 0")
            .When(x => x.UnitCost.HasValue);
    }
}
