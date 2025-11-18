using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Commands.ShipStockTransfer;

public class ShipStockTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public int ShippedQuantity { get; set; }
    public string? TrackingNumber { get; set; }
    public decimal? ShippingCost { get; set; }
    public Guid UserId { get; set; }
}

public class ShipStockTransferCommandValidator : AbstractValidator<ShipStockTransferCommand>
{
    public ShipStockTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.ShippedQuantity)
            .GreaterThan(0).WithMessage("Shipped quantity must be greater than 0");

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.TrackingNumber));

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost must be greater than or equal to 0")
            .When(x => x.ShippingCost.HasValue);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
