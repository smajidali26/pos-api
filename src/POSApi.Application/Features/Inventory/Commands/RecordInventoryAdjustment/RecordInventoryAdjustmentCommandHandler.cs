using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Inventory.Commands.RecordInventoryAdjustment;

public class RecordInventoryAdjustmentCommandHandler : ICommandHandler<RecordInventoryAdjustmentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecordInventoryAdjustmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(RecordInventoryAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        var previousQuantity = product.StockQuantity;
        var quantityChange = request.NewQuantity - previousQuantity;

        // Create inventory movement record
        var movement = new InventoryMovement(
            request.ProductId,
            MovementType.Adjustment,
            quantityChange,
            previousQuantity,
            request.UserId, // This should come from HttpContext or command
            request.Reason,
            request.ReferenceNumber ?? "",
            request.ReferenceId,
            request.UnitCost,
            request.LocationId
        );

        await _unitOfWork.Context.InventoryMovements.AddAsync(movement, cancellationToken);

        // Update product stock
        product.UpdateStock(request.NewQuantity);
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return movement.Id;
    }
}
