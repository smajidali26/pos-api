using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Queries.GetExpiringBatches;

public class GetExpiringBatchesQueryHandler : IQueryHandler<GetExpiringBatchesQuery, IEnumerable<BatchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExpiringBatchesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BatchDto>> Handle(GetExpiringBatchesQuery request, CancellationToken cancellationToken)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(request.DaysThreshold);

        var query = _unitOfWork.Context.Batches
            .Where(b => b.ExpiryDate.HasValue)
            .Where(b => b.ExpiryDate.Value <= thresholdDate)
            .Where(b => b.ExpiryDate.Value > DateTime.UtcNow)
            .Where(b => b.Status == BatchStatus.Active)
            .Where(b => b.CurrentQuantity > 0);

        if (request.ProductId.HasValue)
            query = query.Where(b => b.ProductId == request.ProductId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(b => b.LocationId == request.LocationId.Value);

        var batches = await query
            .Include(b => b.Product)
            .Include(b => b.Vendor)
            .Include(b => b.Location)
            .OrderBy(b => b.ExpiryDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<BatchDto>>(batches);
    }
}
