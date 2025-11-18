using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.SerialNumbers.Queries.GetAllSerialNumbers;

public class GetAllSerialNumbersQuery : IQuery<IEnumerable<SerialNumberDto>>
{
    public Guid? ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? CustomerId { get; set; }
    public SerialNumberStatus? Status { get; set; }
    public bool? IsUnderWarranty { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
