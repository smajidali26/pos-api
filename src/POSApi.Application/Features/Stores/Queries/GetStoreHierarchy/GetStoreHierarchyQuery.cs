using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Queries.GetStoreHierarchy;

public class GetStoreHierarchyQuery : IQuery<List<StoreHierarchyDto>>
{
    public Guid? RootStoreId { get; set; }
}

public class GetStoreHierarchyQueryHandler : IQueryHandler<GetStoreHierarchyQuery, List<StoreHierarchyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStoreHierarchyQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<StoreHierarchyDto>> Handle(GetStoreHierarchyQuery request, CancellationToken cancellationToken)
    {
        var hierarchy = await _unitOfWork.Stores.GetStoreHierarchyAsync(request.RootStoreId, cancellationToken);
        return _mapper.Map<List<StoreHierarchyDto>>(hierarchy);
    }
}
