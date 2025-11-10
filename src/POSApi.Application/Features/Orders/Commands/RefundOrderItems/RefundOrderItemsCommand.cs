using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Orders.Commands.RefundOrderItems;

public class RefundOrderItemsCommand : ICommand<bool>
{
    public Guid OrderId { get; set; }
    public List<RefundItemDto> Items { get; set; } = new();
    public string? Reason { get; set; }
    public Guid ProcessedByUserId { get; set; } // Cashier or Manager processing the refund
}

public class RefundItemDto
{
    public Guid OrderItemId { get; set; }
    public int QuantityToRefund { get; set; }
}

public class RefundOrderItemsCommandValidator : AbstractValidator<RefundOrderItemsCommand>
{
    public RefundOrderItemsCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be refunded")
            .Must(items => items.All(i => i.QuantityToRefund > 0))
            .WithMessage("Refund quantity must be greater than 0");

        RuleFor(x => x.ProcessedByUserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
