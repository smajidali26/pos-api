using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Commands.ShipStockTransfer;

public class ShipStockTransferCommandHandler : ICommandHandler<ShipStockTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ShipStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { request.TransferId }, cancellationToken);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer with ID {request.TransferId} not found");

        transfer.Ship(
            request.UserId,
            request.ShippedQuantity,
            request.TrackingNumber ?? string.Empty,
            request.ShippingCost
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
