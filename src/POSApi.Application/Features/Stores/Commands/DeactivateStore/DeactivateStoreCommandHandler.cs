using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Commands.DeactivateStore;

public class DeactivateStoreCommandHandler : ICommandHandler<DeactivateStoreCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateStoreCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId, cancellationToken);
        if (store == null)
        {
            throw new InvalidOperationException($"Store with ID {request.StoreId} not found");
        }

        store.Deactivate();
        _unitOfWork.Stores.Update(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
