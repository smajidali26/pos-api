using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Queries.GetTransferStatistics;

public class GetTransferStatisticsQuery : IQuery<StockTransferStatisticsDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
