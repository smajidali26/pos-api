using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Queries.GetStoreById;

public class GetStoreByIdQuery : IQuery<StoreDto?>
{
    public Guid StoreId { get; set; }

    public GetStoreByIdQuery(Guid storeId)
    {
        StoreId = storeId;
    }
}

public class GetStoreByIdQueryHandler : IQueryHandler<GetStoreByIdQuery, StoreDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStoreByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId, cancellationToken);
        return store == null ? null : _mapper.Map<StoreDto>(store);
    }
}
