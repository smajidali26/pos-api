using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Queries.GetTransfersByStore;

public class GetTransfersByStoreQuery : IQuery<List<InterStoreTransferDto>>
{
    public Guid StoreId { get; set; }
    public bool IncludeIncoming { get; set; } = true;
    public bool IncludeOutgoing { get; set; } = true;

    public GetTransfersByStoreQuery(Guid storeId)
    {
        StoreId = storeId;
    }
}

public class GetTransfersByStoreQueryHandler : IQueryHandler<GetTransfersByStoreQuery, List<InterStoreTransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransfersByStoreQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<InterStoreTransferDto>> Handle(GetTransfersByStoreQuery request, CancellationToken cancellationToken)
    {
        var transfers = await _unitOfWork.InterStoreTransfers.GetByStoreIdAsync(
            request.StoreId,
            request.IncludeIncoming,
            request.IncludeOutgoing,
            cancellationToken);

        return _mapper.Map<List<InterStoreTransferDto>>(transfers);
    }
}
