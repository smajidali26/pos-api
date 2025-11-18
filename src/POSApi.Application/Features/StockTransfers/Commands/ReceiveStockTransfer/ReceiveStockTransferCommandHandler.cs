using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Commands.ReceiveStockTransfer;

public class ReceiveStockTransferCommandHandler : ICommandHandler<ReceiveStockTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReceiveStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReceiveStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { request.TransferId }, cancellationToken);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer with ID {request.TransferId} not found");

        transfer.Receive(
            request.UserId,
            request.ReceivedQuantity,
            request.ReceiverNotes ?? string.Empty
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
