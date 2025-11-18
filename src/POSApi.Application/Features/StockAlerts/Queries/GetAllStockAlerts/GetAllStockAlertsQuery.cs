using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.StockAlerts.Queries.GetAllStockAlerts;

public class GetAllStockAlertsQuery : IQuery<IEnumerable<StockAlertDto>>
{
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public StockAlertType? AlertType { get; set; }
    public StockAlertSeverity? Severity { get; set; }
    public StockAlertStatus? Status { get; set; }
    public bool? IsOverdue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
