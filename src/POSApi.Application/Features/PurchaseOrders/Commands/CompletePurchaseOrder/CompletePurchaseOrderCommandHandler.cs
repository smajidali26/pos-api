using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder;

public class CompletePurchaseOrderCommandHandler : ICommandHandler<CompletePurchaseOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompletePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompletePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
        }

        // Add completion notes if provided
        if (!string.IsNullOrWhiteSpace(request.CompletionNotes))
        {
            var currentNotes = purchaseOrder.Notes;
            var newNotes = string.IsNullOrWhiteSpace(currentNotes) 
                ? $"Completed: {request.CompletionNotes}" 
                : $"{currentNotes}\n\nCompleted: {request.CompletionNotes}";
            purchaseOrder.UpdateNotes(newNotes);
        }

        purchaseOrder.Complete();

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}