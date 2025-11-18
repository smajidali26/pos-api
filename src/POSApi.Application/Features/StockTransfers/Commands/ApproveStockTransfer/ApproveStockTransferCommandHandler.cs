using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Commands.ApproveStockTransfer;

public class ApproveStockTransferCommandHandler : ICommandHandler<ApproveStockTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApproveStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers.FindAsync(new object[] { request.TransferId }, cancellationToken);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer with ID {request.TransferId} not found");

        transfer.Approve(request.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
