using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Commands.UpdateStoreManager;

public class UpdateStoreManagerCommandHandler : ICommandHandler<UpdateStoreManagerCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStoreManagerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateStoreManagerCommand request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId, cancellationToken);
        if (store == null)
        {
            throw new InvalidOperationException($"Store with ID {request.StoreId} not found");
        }

        var manager = await _unitOfWork.Users.GetByIdAsync(request.ManagerUserId, cancellationToken);
        if (manager == null)
        {
            throw new InvalidOperationException($"User with ID {request.ManagerUserId} not found");
        }

        store.UpdateManager(request.ManagerUserId);
        _unitOfWork.Stores.Update(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
