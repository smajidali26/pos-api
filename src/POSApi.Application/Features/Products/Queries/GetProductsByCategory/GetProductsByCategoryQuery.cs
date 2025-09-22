using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Queries.GetProductsByCategory;

public class GetProductsByCategoryQuery : IQuery<IEnumerable<ProductDto>>
{
    public Guid CategoryId { get; }
    public bool IncludeInactive { get; }

    public GetProductsByCategoryQuery(Guid categoryId, bool includeInactive = false)
    {
        CategoryId = categoryId;
        IncludeInactive = includeInactive;
    }
}