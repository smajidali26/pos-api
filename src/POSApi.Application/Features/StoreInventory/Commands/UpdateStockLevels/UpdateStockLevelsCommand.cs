using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StoreInventory.Commands.UpdateStockLevels;

public class UpdateStockLevelsCommand : ICommand
{
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public int ReorderPoint { get; set; }
}

public class UpdateStockLevelsCommandValidator : AbstractValidator<UpdateStockLevelsCommand>
{
    public UpdateStockLevelsCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be non-negative");

        RuleFor(x => x.MaxStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock level must be non-negative");

        RuleFor(x => x.ReorderPoint)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder point must be non-negative");

        RuleFor(x => x.MaxStockLevel)
            .GreaterThanOrEqualTo(x => x.MinStockLevel)
            .When(x => x.MaxStockLevel > 0)
            .WithMessage("Maximum stock level must be greater than or equal to minimum stock level");
    }
}
