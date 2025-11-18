using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.StockTransfers.Queries.GetStockTransferById;

public class GetStockTransferByIdQuery : IQuery<StockTransferDto?>
{
    public Guid TransferId { get; set; }
}
