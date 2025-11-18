using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Queries.GetProductBatchesFIFO;

public class GetProductBatchesFIFOQuery : IQuery<IEnumerable<BatchDto>>
{
    public Guid ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public bool IncludeExpired { get; set; } = false;
}
