using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Queries.GetProductBatchesFIFO;

public class GetProductBatchesFIFOQueryHandler : IQueryHandler<GetProductBatchesFIFOQuery, IEnumerable<BatchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductBatchesFIFOQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BatchDto>> Handle(GetProductBatchesFIFOQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.Batches
            .Where(b => b.ProductId == request.ProductId)
            .Where(b => b.CurrentQuantity > 0)
            .Where(b => b.Status == BatchStatus.Active);

        if (request.LocationId.HasValue)
            query = query.Where(b => b.LocationId == request.LocationId.Value);

        if (!request.IncludeExpired)
            query = query.Where(b => !b.IsExpired);

        var batches = await query
            .Include(b => b.Product)
            .Include(b => b.Vendor)
            .Include(b => b.Location)
            .OrderBy(b => b.ManufactureDate ?? b.CreatedAt)
            .ThenBy(b => b.ExpiryDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<BatchDto>>(batches);
    }
}
