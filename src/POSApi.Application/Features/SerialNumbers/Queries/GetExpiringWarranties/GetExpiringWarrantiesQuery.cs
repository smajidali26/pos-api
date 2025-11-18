using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.SerialNumbers.Queries.GetExpiringWarranties;

public class GetExpiringWarrantiesQuery : IQuery<IEnumerable<SerialNumberDto>>
{
    public int DaysThreshold { get; set; } = 30;
    public Guid? ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
