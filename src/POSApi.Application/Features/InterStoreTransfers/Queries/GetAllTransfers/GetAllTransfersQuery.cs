using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Queries.GetAllTransfers;

public class GetAllTransfersQuery : IQuery<PagedResult<InterStoreTransferDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public TransferStatus? Status { get; set; }
    public Guid? StoreId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

public class GetAllTransfersQueryHandler : IQueryHandler<GetAllTransfersQuery, PagedResult<InterStoreTransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTransfersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<InterStoreTransferDto>> Handle(GetAllTransfersQuery request, CancellationToken cancellationToken)
    {
        var (transfers, totalCount) = await _unitOfWork.InterStoreTransfers.GetTransfersPagedAsync(
            request.Skip,
            request.Take,
            request.Status,
            request.StoreId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var transferDtos = _mapper.Map<IEnumerable<InterStoreTransferDto>>(transfers);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedResult<InterStoreTransferDto>
        {
            Items = transferDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }
}
