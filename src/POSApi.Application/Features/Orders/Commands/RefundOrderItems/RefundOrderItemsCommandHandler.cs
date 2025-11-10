using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace POSApi.Application.Features.Orders.Commands.RefundOrderItems;

public class RefundOrderItemsCommandHandler : ICommandHandler<RefundOrderItemsCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefundOrderItemsCommandHandler> _logger;

    public RefundOrderItemsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RefundOrderItemsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RefundOrderItemsCommand request, CancellationToken cancellationToken)
    {
        // Get the order with items
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        // Verify order is completed
        if (order.Status != Domain.Entities.OrderStatus.Completed)
        {
            throw new InvalidOperationException("Only completed orders can be refunded");
        }

        // Build refund list with order item IDs and quantities
        var refundedItems = new List<(Guid orderItemId, int quantityRefunded)>();
        var stockRestoration = new List<(Guid productId, int quantity)>();

        foreach (var refundItem in request.Items)
        {
            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == refundItem.OrderItemId);
            if (orderItem == null)
            {
                throw new InvalidOperationException($"Order item {refundItem.OrderItemId} not found in order");
            }

            if (refundItem.QuantityToRefund > orderItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Cannot refund {refundItem.QuantityToRefund} items. Only {orderItem.Quantity} available.");
            }

            refundedItems.Add((refundItem.OrderItemId, refundItem.QuantityToRefund));
            stockRestoration.Add((orderItem.ProductId, refundItem.QuantityToRefund));
        }

        // Process partial refund on the order
        order.PartialRefund(refundedItems, request.Reason);

        // Restore stock for refunded items
        foreach (var (productId, quantity) in stockRestoration)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                product.IncreaseStock(quantity);
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                _logger.LogInformation("Restored {Quantity} units to product {ProductId} due to refund",
                    quantity, productId);
            }
        }

        // Update the order
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} refunded by user {UserId}. Items refunded: {ItemCount}",
            request.OrderId, request.ProcessedByUserId, request.Items.Count);

        return true;
    }
}
