using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Commands.UpdateBatchNotes;

public class UpdateBatchNotesCommandHandler : ICommandHandler<UpdateBatchNotesCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBatchNotesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateBatchNotesCommand request, CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.Context.Batches
            .FindAsync(new object[] { request.BatchId }, cancellationToken);

        if (batch == null)
            throw new InvalidOperationException($"Batch with ID {request.BatchId} not found");

        batch.UpdateNotes(request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
