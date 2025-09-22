using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderCommandHandler : ICommandHandler<CancelPurchaseOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelPurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
        }

        purchaseOrder.Cancel(request.CancellationReason);

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}