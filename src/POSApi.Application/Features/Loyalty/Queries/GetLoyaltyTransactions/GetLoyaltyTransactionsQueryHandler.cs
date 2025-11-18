using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsQueryHandler : IQueryHandler<GetLoyaltyTransactionsQuery, PagedResult<LoyaltyTransactionDto>>
{
    private readonly PosDbContext _context;

    public GetLoyaltyTransactionsQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LoyaltyTransactionDto>> Handle(GetLoyaltyTransactionsQuery request, CancellationToken cancellationToken)
    {
        // Get customer loyalty
        var customerLoyalty = await _context.CustomerLoyalties
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            return new PagedResult<LoyaltyTransactionDto>
            {
                Items = new List<LoyaltyTransactionDto>(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        var query = _context.LoyaltyTransactions
            .Include(t => t.Order)
            .Where(t => t.CustomerLoyaltyId == customerLoyalty.Id)
            .AsQueryable();

        // Apply filters
        if (request.TransactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == request.TransactionType.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new LoyaltyTransactionDto
            {
                Id = t.Id,
                CustomerLoyaltyId = t.CustomerLoyaltyId,
                OrderId = t.OrderId,
                OrderNumber = t.Order != null ? t.Order.OrderNumber : null,
                PointsEarned = t.PointsEarned,
                PointsRedeemed = t.PointsRedeemed,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                TransactionDate = t.TransactionDate,
                Description = t.Description,
                TransactionType = t.TransactionType.ToString(),
                ExpiryDate = t.ExpiryDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<LoyaltyTransactionDto>
        {
            Items = transactions,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
