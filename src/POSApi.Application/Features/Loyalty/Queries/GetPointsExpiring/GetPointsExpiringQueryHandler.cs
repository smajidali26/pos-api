using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetPointsExpiring;

public class GetPointsExpiringQueryHandler : IQueryHandler<GetPointsExpiringQuery, List<LoyaltyTransactionDto>>
{
    private readonly PosDbContext _context;

    public GetPointsExpiringQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<LoyaltyTransactionDto>> Handle(GetPointsExpiringQuery request, CancellationToken cancellationToken)
    {
        var customerLoyalty = await _context.CustomerLoyalties
            .Include(cl => cl.Transactions)
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            return new List<LoyaltyTransactionDto>();
        }

        var expiringTransactions = customerLoyalty.GetExpiringTransactions(request.DaysThreshold);

        return expiringTransactions
            .Select(t => new LoyaltyTransactionDto
            {
                Id = t.Id,
                CustomerLoyaltyId = t.CustomerLoyaltyId,
                OrderId = t.OrderId,
                OrderNumber = null,
                PointsEarned = t.PointsEarned,
                PointsRedeemed = t.PointsRedeemed,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                TransactionDate = t.TransactionDate,
                Description = t.Description,
                TransactionType = t.TransactionType.ToString(),
                ExpiryDate = t.ExpiryDate
            })
            .ToList();
    }
}
