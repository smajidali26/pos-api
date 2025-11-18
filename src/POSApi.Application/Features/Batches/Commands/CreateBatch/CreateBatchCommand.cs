using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Commands.CreateBatch;

public class CreateBatchCommand : ICommand<Guid>
{
    public string BatchNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? LocationId { get; set; }
    public int InitialQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateBatchCommandValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchCommandValidator()
    {
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.InitialQuantity)
            .GreaterThan(0).WithMessage("Initial quantity must be greater than 0");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost must be greater than or equal to 0");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.ManufactureDate ?? DateTime.MinValue)
            .WithMessage("Expiry date must be after manufacture date")
            .When(x => x.ManufactureDate.HasValue && x.ExpiryDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
