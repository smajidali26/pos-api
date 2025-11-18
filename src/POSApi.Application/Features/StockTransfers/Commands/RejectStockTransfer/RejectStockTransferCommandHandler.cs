using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Commands.RejectStockTransfer;

public class RejectStockTransferCommandHandler : ICommandHandler<RejectStockTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RejectStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { request.TransferId }, cancellationToken);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer with ID {request.TransferId} not found");

        transfer.Reject(request.UserId, request.RejectionReason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
