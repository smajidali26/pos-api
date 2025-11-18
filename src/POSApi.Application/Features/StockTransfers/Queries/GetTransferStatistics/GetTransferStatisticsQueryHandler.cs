using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Queries.GetTransferStatistics;

public class GetTransferStatisticsQueryHandler : IQueryHandler<GetTransferStatisticsQuery, StockTransferStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTransferStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StockTransferStatisticsDto> Handle(GetTransferStatisticsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.StockTransfers.AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(t => t.RequestedDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(t => t.RequestedDate <= request.EndDate.Value);

        var transfers = await query.ToListAsync(cancellationToken);

        var pendingCount = transfers.Count(t => t.Status == TransferStatus.Pending);
        var approvedCount = transfers.Count(t => t.Status == TransferStatus.Approved);
        var inTransitCount = transfers.Count(t => t.Status == TransferStatus.InTransit);
        var completedCount = transfers.Count(t => t.Status == TransferStatus.Completed);
        var rejectedCount = transfers.Count(t => t.Status == TransferStatus.Rejected);
        var cancelledCount = transfers.Count(t => t.Status == TransferStatus.Cancelled);
        var totalWithVariance = transfers.Count(t => t.HasVariance);

        // Calculate average transfer time for completed transfers
        var completedTransfers = transfers.Where(t => t.Status == TransferStatus.Completed && t.ReceivedDate.HasValue).ToList();
        var averageTransferTime = completedTransfers.Any()
            ? completedTransfers.Average(t => (t.ReceivedDate!.Value - t.RequestedDate).TotalHours)
            : 0;

        return new StockTransferStatisticsDto
        {
            PendingCount = pendingCount,
            ApprovedCount = approvedCount,
            InTransitCount = inTransitCount,
            CompletedCount = completedCount,
            RejectedCount = rejectedCount,
            CancelledCount = cancelledCount,
            TotalWithVariance = totalWithVariance,
            AverageTransferTime = (decimal)averageTransferTime
        };
    }
}
