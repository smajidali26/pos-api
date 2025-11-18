using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Queries.GetAllBatches;

public class GetAllBatchesQueryHandler : IQueryHandler<GetAllBatchesQuery, IEnumerable<BatchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllBatchesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BatchDto>> Handle(GetAllBatchesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.Batches.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(b => b.ProductId == request.ProductId.Value);

        if (request.VendorId.HasValue)
            query = query.Where(b => b.VendorId == request.VendorId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(b => b.LocationId == request.LocationId.Value);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        if (request.IsExpired.HasValue)
            query = query.Where(b => b.IsExpired == request.IsExpired.Value);

        if (request.IsExpiringSoon.HasValue)
            query = query.Where(b => b.IsExpiringSoon == request.IsExpiringSoon.Value);

        if (request.StartDate.HasValue)
            query = query.Where(b => b.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(b => b.CreatedAt <= request.EndDate.Value);

        var batches = await query
            .Include(b => b.Product)
            .Include(b => b.Vendor)
            .Include(b => b.Location)
            .Include(b => b.RecalledBy)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<BatchDto>>(batches);
    }
}
