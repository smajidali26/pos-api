using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.DeactivateProduct;

public class DeactivateProductCommand : ICommand
{
    public Guid ProductId { get; set; }
}

public class DeactivateProductCommandValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}