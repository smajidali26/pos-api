using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Commands.CreateStore;

public class CreateStoreCommandHandler : ICommandHandler<CreateStoreCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStoreCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        // Validate manager exists
        var manager = await _unitOfWork.Users.GetByIdAsync(request.ManagerUserId, cancellationToken);
        if (manager == null)
        {
            throw new InvalidOperationException($"Manager with ID {request.ManagerUserId} not found");
        }

        // Validate store code is unique
        var existingStore = await _unitOfWork.Stores.GetByCodeAsync(request.Code, cancellationToken);
        if (existingStore != null)
        {
            throw new InvalidOperationException($"Store with code '{request.Code}' already exists");
        }

        // Validate parent store exists if specified
        if (request.ParentStoreId.HasValue)
        {
            var parentStore = await _unitOfWork.Stores.GetByIdAsync(request.ParentStoreId.Value, cancellationToken);
            if (parentStore == null)
            {
                throw new InvalidOperationException($"Parent store with ID {request.ParentStoreId} not found");
            }
        }

        // Create store
        var store = new Store(
            request.Name,
            request.Code,
            request.Address,
            request.City,
            request.State,
            request.ZipCode,
            request.Country,
            request.ManagerUserId,
            request.StoreType,
            request.ParentStoreId,
            request.PhoneNumber,
            request.Email);

        // Set optional operating hours
        if (request.OpenTime.HasValue && request.CloseTime.HasValue)
        {
            store.UpdateOperatingHours(
                request.OpenTime.Value,
                request.CloseTime.Value,
                request.TimeZone ?? "UTC");
        }

        // Set optional financial settings
        if (request.TaxRate.HasValue || !string.IsNullOrEmpty(request.Currency))
        {
            store.UpdateFinancialSettings(
                request.TaxRate ?? 0.08m,
                request.Currency ?? "USD");
        }

        // Update notes if provided
        if (!string.IsNullOrEmpty(request.Notes))
        {
            store.UpdateDetails(
                request.Name,
                request.Address,
                request.City,
                request.State,
                request.ZipCode,
                request.PhoneNumber,
                request.Email,
                request.Notes);
        }

        await _unitOfWork.Stores.AddAsync(store, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return store.Id;
    }
}
