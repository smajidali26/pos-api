using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Commands.CancelStockTransfer;

public class CancelStockTransferCommandHandler : ICommandHandler<CancelStockTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { request.TransferId }, cancellationToken);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer with ID {request.TransferId} not found");

        transfer.Cancel(request.UserId, request.CancellationReason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
