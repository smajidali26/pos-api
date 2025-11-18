using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Commands.UpdateStore;

public class UpdateStoreCommandHandler : ICommandHandler<UpdateStoreCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStoreCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.Id, cancellationToken);
        if (store == null)
        {
            throw new InvalidOperationException($"Store with ID {request.Id} not found");
        }

        // Validate parent store if specified
        if (request.ParentStoreId.HasValue)
        {
            if (request.ParentStoreId.Value == request.Id)
            {
                throw new InvalidOperationException("A store cannot be its own parent");
            }

            var parentStore = await _unitOfWork.Stores.GetByIdAsync(request.ParentStoreId.Value, cancellationToken);
            if (parentStore == null)
            {
                throw new InvalidOperationException($"Parent store with ID {request.ParentStoreId} not found");
            }
        }

        // Update basic details
        store.UpdateDetails(
            request.Name,
            request.Address,
            request.City,
            request.State,
            request.ZipCode,
            request.PhoneNumber,
            request.Email,
            request.Notes);

        // Update store type if specified
        if (request.StoreType.HasValue)
        {
            store.UpdateStoreType(request.StoreType.Value);
        }

        // Update parent store
        store.SetParentStore(request.ParentStoreId);

        // Update operating hours if specified
        if (request.OpenTime.HasValue && request.CloseTime.HasValue)
        {
            store.UpdateOperatingHours(
                request.OpenTime.Value,
                request.CloseTime.Value,
                request.TimeZone ?? "UTC");
        }

        // Update financial settings if specified
        if (request.TaxRate.HasValue || !string.IsNullOrEmpty(request.Currency))
        {
            var currentTaxRate = request.TaxRate ?? 0.08m;
            var currentCurrency = request.Currency ?? "USD";
            store.UpdateFinancialSettings(currentTaxRate, currentCurrency);
        }

        _unitOfWork.Stores.Update(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
