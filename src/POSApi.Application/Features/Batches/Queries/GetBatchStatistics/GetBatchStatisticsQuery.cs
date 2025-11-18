using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Queries.GetBatchStatistics;

public class GetBatchStatisticsQuery : IQuery<BatchStatisticsDto>
{
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
}
