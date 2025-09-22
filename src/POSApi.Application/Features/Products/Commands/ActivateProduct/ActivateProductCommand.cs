using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.ActivateProduct;

public class ActivateProductCommand : ICommand
{
    public Guid ProductId { get; set; }
}

public class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}