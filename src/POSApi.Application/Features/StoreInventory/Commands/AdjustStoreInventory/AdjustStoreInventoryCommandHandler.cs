using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StoreInventory.Commands.AdjustStoreInventory;

public class AdjustStoreInventoryCommandHandler : ICommandHandler<AdjustStoreInventoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AdjustStoreInventoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AdjustStoreInventoryCommand request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId, cancellationToken);
        if (store == null)
        {
            throw new InvalidOperationException($"Store with ID {request.StoreId} not found");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        var inventory = await _unitOfWork.StoreInventories.GetByStoreAndProductAsync(
            request.StoreId,
            request.ProductId,
            cancellationToken);

        if (inventory == null)
        {
            // Create new inventory record if it doesn't exist
            inventory = new Domain.Entities.StoreInventory(request.StoreId, request.ProductId);
            await _unitOfWork.StoreInventories.AddAsync(inventory, cancellationToken);
        }

        inventory.AdjustQuantity(request.AdjustmentAmount);
        _unitOfWork.StoreInventories.Update(inventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
