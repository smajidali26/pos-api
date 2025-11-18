using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Queries.GetExpiringBatches;

public class GetExpiringBatchesQuery : IQuery<IEnumerable<BatchDto>>
{
    public int DaysThreshold { get; set; } = 30;
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
