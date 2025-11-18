using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Commands.RepairSerialNumber;

public class RepairSerialNumberCommandHandler : ICommandHandler<RepairSerialNumberCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RepairSerialNumberCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RepairSerialNumberCommand request, CancellationToken cancellationToken)
    {
        var serialNumber = await _unitOfWork.Context.SerialNumbers
            .FindAsync(new object[] { request.SerialNumberId }, cancellationToken);

        if (serialNumber == null)
            throw new InvalidOperationException($"Serial number with ID {request.SerialNumberId} not found");

        serialNumber.Repair(request.UserId, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
