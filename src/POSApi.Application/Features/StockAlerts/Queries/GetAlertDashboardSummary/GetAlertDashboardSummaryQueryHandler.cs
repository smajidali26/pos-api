using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockAlerts.Queries.GetAlertDashboardSummary;

public class GetAlertDashboardSummaryQueryHandler : IQueryHandler<GetAlertDashboardSummaryQuery, StockAlertSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAlertDashboardSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StockAlertSummaryDto> Handle(GetAlertDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.StockAlerts.AsQueryable();

        if (request.LocationId.HasValue)
            query = query.Where(a => a.LocationId == request.LocationId.Value);

        var alerts = await query.ToListAsync(cancellationToken);

        var activeAlerts = alerts.Where(a => a.Status == StockAlertStatus.Active).ToList();

        var summary = new StockAlertSummaryDto
        {
            TotalActiveAlerts = activeAlerts.Count,
            CriticalAlerts = activeAlerts.Count(a => a.Severity == StockAlertSeverity.Critical),
            HighPriorityAlerts = activeAlerts.Count(a => a.Severity == StockAlertSeverity.High),
            MediumPriorityAlerts = activeAlerts.Count(a => a.Severity == StockAlertSeverity.Medium),
            LowPriorityAlerts = activeAlerts.Count(a => a.Severity == StockAlertSeverity.Low),
            OverdueAlerts = activeAlerts.Count(a => a.IsOverdue),
            OutOfStockAlerts = activeAlerts.Count(a => a.AlertType == StockAlertType.OutOfStock),
            LowStockAlerts = activeAlerts.Count(a => a.AlertType == StockAlertType.LowStock),
            ExpiringAlerts = activeAlerts.Count(a => a.AlertType == StockAlertType.ExpiringBatch)
        };

        return summary;
    }
}
