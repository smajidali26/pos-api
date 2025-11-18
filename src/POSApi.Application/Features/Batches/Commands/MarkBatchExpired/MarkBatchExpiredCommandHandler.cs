using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Commands.MarkBatchExpired;

public class MarkBatchExpiredCommandHandler : ICommandHandler<MarkBatchExpiredCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkBatchExpiredCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkBatchExpiredCommand request, CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.Context.Batches
            .FindAsync(new object[] { request.BatchId }, cancellationToken);

        if (batch == null)
            throw new InvalidOperationException($"Batch with ID {request.BatchId} not found");

        batch.MarkAsExpired();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
