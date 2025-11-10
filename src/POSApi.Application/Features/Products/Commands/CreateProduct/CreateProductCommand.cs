using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.DTOs;

namespace POSApi.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? SizeId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? PrimaryVendorId { get; set; }

    // Unit of Measure Properties
    public string BaseUnitCode { get; set; } = "PCS"; // Default to pieces
    public decimal BaseQuantity { get; set; } = 1m;
    public string? PackagingUnitCode { get; set; }
    public decimal? PackagingQuantity { get; set; }

    // Physical Properties (optional)
    public decimal? Weight { get; set; }
    public string? WeightUnitCode { get; set; }
    public decimal? Volume { get; set; }
    public string? VolumeUnitCode { get; set; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be greater than or equal to 0");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        // UOM Validations
        RuleFor(x => x.BaseUnitCode)
            .NotEmpty().WithMessage("Base unit is required");

        RuleFor(x => x.BaseQuantity)
            .GreaterThan(0).WithMessage("Base quantity must be greater than 0");

        // Conditional validation for packaging
        RuleFor(x => x.PackagingQuantity)
            .GreaterThan(0).WithMessage("Packaging quantity must be greater than 0")
            .When(x => !string.IsNullOrEmpty(x.PackagingUnitCode));

        RuleFor(x => x.PackagingUnitCode)
            .NotEmpty().WithMessage("Packaging unit code is required when packaging quantity is specified")
            .When(x => x.PackagingQuantity.HasValue);

        // Physical property validations
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.WeightUnitCode)
            .NotEmpty().WithMessage("Weight unit is required when weight is specified")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Volume)
            .GreaterThan(0).WithMessage("Volume must be greater than 0")
            .When(x => x.Volume.HasValue);

        RuleFor(x => x.VolumeUnitCode)
            .NotEmpty().WithMessage("Volume unit is required when volume is specified")
            .When(x => x.Volume.HasValue);
    }
}