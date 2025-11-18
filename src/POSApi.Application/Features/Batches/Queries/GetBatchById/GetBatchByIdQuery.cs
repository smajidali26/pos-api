using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Queries.GetBatchById;

public class GetBatchByIdQuery : IQuery<BatchDto>
{
    public Guid BatchId { get; set; }
}
