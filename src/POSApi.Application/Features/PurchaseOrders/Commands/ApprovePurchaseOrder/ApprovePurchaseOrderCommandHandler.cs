using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommandHandler : ICommandHandler<ApprovePurchaseOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
        }

        // Verify approver exists
        var approver = await _unitOfWork.Users.GetByIdAsync(request.ApprovedByUserId, cancellationToken);
        if (approver == null)
        {
            throw new InvalidOperationException($"Approver with ID {request.ApprovedByUserId} not found");
        }

        // Business rule: Could add approval authority checks here
        // For now, we'll allow any valid user to approve

        purchaseOrder.Approve();

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}