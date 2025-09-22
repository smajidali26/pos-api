using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int MinStockLevel { get; set; }
    public Guid CategoryId { get; set; }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be greater than or equal to 0");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");
    }
}