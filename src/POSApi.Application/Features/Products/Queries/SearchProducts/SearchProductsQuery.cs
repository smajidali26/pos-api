using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Queries.SearchProducts;

public class SearchProductsQuery : IQuery<IEnumerable<ProductDto>>
{
    public string SearchTerm { get; }
    public Guid? CategoryId { get; }
    public bool? IsActive { get; }
    public bool? IsLowStock { get; }

    public SearchProductsQuery(string searchTerm, Guid? categoryId = null, bool? isActive = null, bool? isLowStock = null)
    {
        SearchTerm = searchTerm ?? string.Empty;
        CategoryId = categoryId;
        IsActive = isActive;
        IsLowStock = isLowStock;
    }
}