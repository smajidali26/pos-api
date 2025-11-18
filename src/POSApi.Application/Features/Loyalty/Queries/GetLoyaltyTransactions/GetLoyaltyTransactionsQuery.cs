using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Loyalty.Queries.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsQuery : IQuery<PagedResult<LoyaltyTransactionDto>>
{
    public Guid CustomerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public LoyaltyTransactionType? TransactionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
