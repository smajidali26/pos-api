using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Stores.Commands.ActivateStore;

public class ActivateStoreCommand : ICommand
{
    public Guid StoreId { get; set; }
}
