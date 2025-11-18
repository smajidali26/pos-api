using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Commands.MarkSerialNumberDefective;

public class MarkSerialNumberDefectiveCommandHandler : ICommandHandler<MarkSerialNumberDefectiveCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkSerialNumberDefectiveCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkSerialNumberDefectiveCommand request, CancellationToken cancellationToken)
    {
        var serialNumber = await _unitOfWork.Context.SerialNumbers
            .FindAsync(new object[] { request.SerialNumberId }, cancellationToken);

        if (serialNumber == null)
            throw new InvalidOperationException($"Serial number with ID {request.SerialNumberId} not found");

        serialNumber.MarkAsDefective(request.UserId, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
