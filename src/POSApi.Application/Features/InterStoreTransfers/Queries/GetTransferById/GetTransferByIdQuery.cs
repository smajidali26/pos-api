using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Queries.GetTransferById;

public class GetTransferByIdQuery : IQuery<InterStoreTransferDto?>
{
    public Guid TransferId { get; set; }

    public GetTransferByIdQuery(Guid transferId)
    {
        TransferId = transferId;
    }
}

public class GetTransferByIdQueryHandler : IQueryHandler<GetTransferByIdQuery, InterStoreTransferDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransferByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InterStoreTransferDto?> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        return transfer == null ? null : _mapper.Map<InterStoreTransferDto>(transfer);
    }
}
