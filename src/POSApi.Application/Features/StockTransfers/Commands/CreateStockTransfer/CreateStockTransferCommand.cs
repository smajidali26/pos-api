using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Commands.CreateStockTransfer;

public class CreateStockTransferCommand : ICommand<Guid>
{
    public Guid ProductId { get; set; }
    public Guid FromLocationId { get; set; }
    public Guid ToLocationId { get; set; }
    public int RequestedQuantity { get; set; }
    public string? Notes { get; set; }
    public Guid UserId { get; set; }
}

public class CreateStockTransferCommandValidator : AbstractValidator<CreateStockTransferCommand>
{
    public CreateStockTransferCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.FromLocationId)
            .NotEmpty().WithMessage("From location is required");

        RuleFor(x => x.ToLocationId)
            .NotEmpty().WithMessage("To location is required")
            .NotEqual(x => x.FromLocationId).WithMessage("From and To locations must be different");

        RuleFor(x => x.RequestedQuantity)
            .GreaterThan(0).WithMessage("Requested quantity must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
