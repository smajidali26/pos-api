using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Commands.SubmitPurchaseOrder;

public class SubmitPurchaseOrderCommandHandler : ICommandHandler<SubmitPurchaseOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
        }

        purchaseOrder.Submit();

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}