using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Commands.ActivateStore;

public class ActivateStoreCommandHandler : ICommandHandler<ActivateStoreCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateStoreCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId, cancellationToken);
        if (store == null)
        {
            throw new InvalidOperationException($"Store with ID {request.StoreId} not found");
        }

        store.Activate();
        _unitOfWork.Stores.Update(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
