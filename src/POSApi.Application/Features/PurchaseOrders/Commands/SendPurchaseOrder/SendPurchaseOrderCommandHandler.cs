using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public class SendPurchaseOrderCommandHandler : ICommandHandler<SendPurchaseOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SendPurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SendPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
        }

        purchaseOrder.Send();

        // Update vendor's last order date
        if (purchaseOrder.Vendor != null)
        {
            purchaseOrder.Vendor.RecordOrder(DateTime.UtcNow, purchaseOrder.TotalAmount);
            await _unitOfWork.Vendors.UpdateAsync(purchaseOrder.Vendor, cancellationToken);
        }

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}