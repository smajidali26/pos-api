using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Queries.GetAllStockTransfers;

public class GetAllStockTransfersQueryHandler : IQueryHandler<GetAllStockTransfersQuery, IEnumerable<StockTransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStockTransfersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StockTransferDto>> Handle(GetAllStockTransfersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.StockTransfers.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(t => t.ProductId == request.ProductId.Value);

        if (request.FromLocationId.HasValue)
            query = query.Where(t => t.FromLocationId == request.FromLocationId.Value);

        if (request.ToLocationId.HasValue)
            query = query.Where(t => t.ToLocationId == request.ToLocationId.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.StartDate.HasValue)
            query = query.Where(t => t.RequestedDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(t => t.RequestedDate <= request.EndDate.Value);

        var transfers = await query
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Include(t => t.RequestedBy)
            .Include(t => t.ApprovedBy)
            .Include(t => t.ShippedBy)
            .Include(t => t.ReceivedBy)
            .OrderByDescending(t => t.RequestedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<StockTransferDto>>(transfers);
    }
}
