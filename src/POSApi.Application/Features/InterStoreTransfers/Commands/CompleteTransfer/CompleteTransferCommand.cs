using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.CompleteTransfer;

public class CompleteTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
}

public class CompleteTransferCommandHandler : ICommandHandler<CompleteTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            throw new InvalidOperationException($"Transfer with ID {request.TransferId} not found");
        }

        transfer.Complete();

        // Update inventories
        foreach (var item in transfer.TransferItems)
        {
            // Reduce from source store
            var fromInventory = await _unitOfWork.StoreInventories.GetByStoreAndProductAsync(
                transfer.FromStoreId, item.ProductId, cancellationToken);

            if (fromInventory != null)
            {
                fromInventory.AdjustQuantity(-item.ReceivedQuantity);
                _unitOfWork.StoreInventories.Update(fromInventory);
            }

            // Add to destination store
            var toInventory = await _unitOfWork.StoreInventories.GetByStoreAndProductAsync(
                transfer.ToStoreId, item.ProductId, cancellationToken);

            if (toInventory == null)
            {
                toInventory = new Domain.Entities.StoreInventory(transfer.ToStoreId, item.ProductId);
                await _unitOfWork.StoreInventories.AddAsync(toInventory, cancellationToken);
            }

            toInventory.AdjustQuantity(item.ReceivedQuantity);
            _unitOfWork.StoreInventories.Update(toInventory);
        }

        _unitOfWork.InterStoreTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
