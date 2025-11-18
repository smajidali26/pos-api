using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.ShipTransfer;

public class ShipTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
}

public class ShipTransferCommandHandler : ICommandHandler<ShipTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ShipTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            throw new InvalidOperationException($"Transfer with ID {request.TransferId} not found");
        }

        transfer.Ship();
        _unitOfWork.InterStoreTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
