using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Stores.Commands.DeactivateStore;

public class DeactivateStoreCommand : ICommand
{
    public Guid StoreId { get; set; }
}
