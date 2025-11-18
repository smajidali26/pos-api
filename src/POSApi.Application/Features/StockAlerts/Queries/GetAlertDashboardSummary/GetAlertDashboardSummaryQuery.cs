using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockAlerts.Queries.GetAlertDashboardSummary;

public class GetAlertDashboardSummaryQuery : IQuery<StockAlertSummaryDto>
{
    public Guid? LocationId { get; set; }
}
