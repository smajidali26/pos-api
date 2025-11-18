using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StoreInventory.Commands.UpdateStockLevels;

public class UpdateStockLevelsCommandHandler : ICommandHandler<UpdateStockLevelsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockLevelsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateStockLevelsCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _unitOfWork.StoreInventories.GetByStoreAndProductAsync(
            request.StoreId,
            request.ProductId,
            cancellationToken);

        if (inventory == null)
        {
            throw new InvalidOperationException($"Inventory record not found for Store {request.StoreId} and Product {request.ProductId}");
        }

        inventory.UpdateStockLevels(request.MinStockLevel, request.MaxStockLevel, request.ReorderPoint);
        _unitOfWork.StoreInventories.Update(inventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
