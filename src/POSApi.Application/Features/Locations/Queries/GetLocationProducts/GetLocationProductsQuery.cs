using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Locations.Queries.GetLocationProducts;

public class GetLocationProductsQuery : IQuery<IEnumerable<ProductLocationDto>>
{
    public Guid LocationId { get; set; }
    public bool? IsLowStock { get; set; }
    public bool? IsOutOfStock { get; set; }
    public bool? IsOverStock { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
