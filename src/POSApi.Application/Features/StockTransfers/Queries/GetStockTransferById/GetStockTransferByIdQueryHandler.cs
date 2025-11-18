using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Queries.GetStockTransferById;

public class GetStockTransferByIdQueryHandler : IQueryHandler<GetStockTransferByIdQuery, StockTransferDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStockTransferByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StockTransferDto?> Handle(GetStockTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.Context.StockTransfers
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Include(t => t.RequestedBy)
            .Include(t => t.ApprovedBy)
            .Include(t => t.ShippedBy)
            .Include(t => t.ReceivedBy)
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            return null;

        return _mapper.Map<StockTransferDto>(transfer);
    }
}
