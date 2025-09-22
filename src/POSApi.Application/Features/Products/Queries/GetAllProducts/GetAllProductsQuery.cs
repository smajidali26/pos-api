using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQuery : IQuery<PagedResult<ProductDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLowStock { get; set; }
    public bool IncludeInactive { get; set; } = false;

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Determine the active status filter
        bool? activeFilter = request.IncludeInactive ? null : true;
        if (request.IsActive.HasValue)
        {
            activeFilter = request.IsActive.Value;
        }

        // Get products with pagination and search
        var (products, totalCount) = await _unitOfWork.Products.GetProductsPagedAsync(
            request.Skip,
            request.Take,
            request.SearchTerm,
            request.CategoryId,
            activeFilter,
            request.IsLowStock,
            cancellationToken);

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }
}