using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.SubmitTransfer;

public class SubmitTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
}

public class SubmitTransferCommandHandler : ICommandHandler<SubmitTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SubmitTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            throw new InvalidOperationException($"Transfer with ID {request.TransferId} not found");
        }

        transfer.Submit();
        _unitOfWork.InterStoreTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
