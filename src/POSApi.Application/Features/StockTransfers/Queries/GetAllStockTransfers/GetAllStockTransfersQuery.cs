using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.StockTransfers.Queries.GetAllStockTransfers;

public class GetAllStockTransfersQuery : IQuery<IEnumerable<StockTransferDto>>
{
    public Guid? ProductId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public TransferStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
