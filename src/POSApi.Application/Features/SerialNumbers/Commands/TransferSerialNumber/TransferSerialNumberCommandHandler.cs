using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Commands.TransferSerialNumber;

public class TransferSerialNumberCommandHandler : ICommandHandler<TransferSerialNumberCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransferSerialNumberCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransferSerialNumberCommand request, CancellationToken cancellationToken)
    {
        var serialNumber = await _unitOfWork.Context.SerialNumbers
            .FindAsync(new object[] { request.SerialNumberId }, cancellationToken);

        if (serialNumber == null)
            throw new InvalidOperationException($"Serial number with ID {request.SerialNumberId} not found");

        var location = await _unitOfWork.Context.Locations
            .FindAsync(new object[] { request.ToLocationId }, cancellationToken);

        if (location == null)
            throw new InvalidOperationException($"Location with ID {request.ToLocationId} not found");

        serialNumber.Transfer(request.ToLocationId, request.UserId, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
