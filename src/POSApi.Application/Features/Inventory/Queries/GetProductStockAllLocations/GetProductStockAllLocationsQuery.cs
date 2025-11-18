using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Inventory.Queries.GetProductStockAllLocations;

public class GetProductStockAllLocationsQuery : IQuery<IEnumerable<ProductLocationDto>>
{
    public Guid ProductId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}
