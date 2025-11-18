using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Queries.GetBatchStatistics;

public class GetBatchStatisticsQueryHandler : IQueryHandler<GetBatchStatisticsQuery, BatchStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBatchStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BatchStatisticsDto> Handle(GetBatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.Batches.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(b => b.ProductId == request.ProductId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(b => b.LocationId == request.LocationId.Value);

        var batches = await query.ToListAsync(cancellationToken);

        var statistics = new BatchStatisticsDto
        {
            TotalActiveBatches = batches.Count(b => b.Status == BatchStatus.Active),
            ExpiredBatches = batches.Count(b => b.IsExpired),
            ExpiringSoonBatches = batches.Count(b => b.IsExpiringSoon && !b.IsExpired),
            RecalledBatches = batches.Count(b => b.Status == BatchStatus.Recalled),
            DepletedBatches = batches.Count(b => b.CurrentQuantity == 0),
            TotalBatchValue = batches
                .Where(b => b.Status == BatchStatus.Active)
                .Sum(b => b.TotalValue)
        };

        return statistics;
    }
}
