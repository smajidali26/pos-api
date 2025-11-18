using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Queries.GetPendingTransfers;

public class GetPendingTransfersQuery : IQuery<List<InterStoreTransferDto>>
{
    public Guid? StoreId { get; set; }
}

public class GetPendingTransfersQueryHandler : IQueryHandler<GetPendingTransfersQuery, List<InterStoreTransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPendingTransfersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<InterStoreTransferDto>> Handle(GetPendingTransfersQuery request, CancellationToken cancellationToken)
    {
        var transfers = await _unitOfWork.InterStoreTransfers.GetByStatusAsync(
            TransferStatus.Pending,
            request.StoreId,
            cancellationToken);

        return _mapper.Map<List<InterStoreTransferDto>>(transfers);
    }
}
