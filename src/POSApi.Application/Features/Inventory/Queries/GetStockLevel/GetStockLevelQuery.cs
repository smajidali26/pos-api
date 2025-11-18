using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Inventory.Queries.GetStockLevel;

public class GetStockLevelQuery : IQuery<ProductLocationDto>
{
    public Guid ProductId { get; set; }
    public Guid? LocationId { get; set; }
}
