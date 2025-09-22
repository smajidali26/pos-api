using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.UpdateProductPricing;

public class UpdateProductPricingCommand : ICommand
{
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}

public class UpdateProductPricingCommandValidator : AbstractValidator<UpdateProductPricingCommand>
{
    public UpdateProductPricingCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0");
    }
}