using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.UpdateStock;

public class UpdateStockCommand : ICommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");
    }
}