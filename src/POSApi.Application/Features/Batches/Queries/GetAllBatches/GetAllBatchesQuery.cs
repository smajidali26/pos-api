using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Batches.Queries.GetAllBatches;

public class GetAllBatchesQuery : IQuery<IEnumerable<BatchDto>>
{
    public Guid? ProductId { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? LocationId { get; set; }
    public BatchStatus? Status { get; set; }
    public bool? IsExpired { get; set; }
    public bool? IsExpiringSoon { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
