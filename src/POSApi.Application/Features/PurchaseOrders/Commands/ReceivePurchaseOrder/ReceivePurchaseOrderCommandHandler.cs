using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommandHandler : ICommandHandler<ReceivePurchaseOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReceivePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
        }

        // Validate that all received products are in the purchase order
        foreach (var (productId, quantity) in request.ReceivedQuantities)
        {
            var orderItem = purchaseOrder.Items.FirstOrDefault(i => i.ProductId == productId);
            if (orderItem == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} is not in this purchase order");
            }

            if (orderItem.ReceivedQuantity + quantity > orderItem.Quantity)
            {
                throw new InvalidOperationException($"Cannot receive more than ordered quantity for product {productId}");
            }
        }

        // Process the receipt
        purchaseOrder.ReceivePartial(request.ReceivedQuantities);

        // Update inventory for received items
        foreach (var (productId, receivedQuantity) in request.ReceivedQuantities)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                var orderItem = purchaseOrder.Items.First(i => i.ProductId == productId);
                
                // Increase product stock
                product.IncreaseStock(receivedQuantity);
                
                // Record purchase information
                product.RecordPurchase(orderItem.UnitCost, DateTime.UtcNow);
                
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            }
        }

        // Update notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var currentNotes = purchaseOrder.Notes;
            var newNotes = string.IsNullOrWhiteSpace(currentNotes) 
                ? request.Notes 
                : $"{currentNotes}\n\nReceipt Notes: {request.Notes}";
            purchaseOrder.UpdateNotes(newNotes);
        }

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}