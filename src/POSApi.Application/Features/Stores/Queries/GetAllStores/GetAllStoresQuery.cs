using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Queries.GetAllStores;

public class GetAllStoresQuery : IQuery<PagedResult<StoreDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentStoreId { get; set; }

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

public class GetAllStoresQueryHandler : IQueryHandler<GetAllStoresQuery, PagedResult<StoreDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStoresQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<StoreDto>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        var (stores, totalCount) = await _unitOfWork.Stores.GetStoresPagedAsync(
            request.Skip,
            request.Take,
            request.SearchTerm,
            request.IsActive,
            request.ParentStoreId,
            cancellationToken);

        var storeDtos = _mapper.Map<IEnumerable<StoreDto>>(stores);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedResult<StoreDto>
        {
            Items = storeDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }
}
