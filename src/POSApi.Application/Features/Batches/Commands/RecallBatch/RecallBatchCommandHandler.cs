using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Commands.RecallBatch;

public class RecallBatchCommandHandler : ICommandHandler<RecallBatchCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecallBatchCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RecallBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.Context.Batches
            .FindAsync(new object[] { request.BatchId }, cancellationToken);

        if (batch == null)
            throw new InvalidOperationException($"Batch with ID {request.BatchId} not found");

        batch.Recall(request.RecallReason, request.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
