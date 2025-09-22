using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Queries.GetLowStockProducts;

public class GetLowStockProductsQuery : IQuery<IEnumerable<ProductDto>>
{
    public bool IncludeOutOfStock { get; set; } = true;
}