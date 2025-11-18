using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetCustomerRedemptions;

public class GetCustomerRedemptionsQueryHandler : IQueryHandler<GetCustomerRedemptionsQuery, List<RewardRedemptionDto>>
{
    private readonly PosDbContext _context;

    public GetCustomerRedemptionsQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<RewardRedemptionDto>> Handle(GetCustomerRedemptionsQuery request, CancellationToken cancellationToken)
    {
        var customerLoyalty = await _context.CustomerLoyalties
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            return new List<RewardRedemptionDto>();
        }

        var query = _context.RewardRedemptions
            .Include(rr => rr.Reward)
            .Include(rr => rr.Order)
            .Where(rr => rr.CustomerLoyaltyId == customerLoyalty.Id)
            .AsQueryable();

        if (request.IncludeUsed == false)
        {
            query = query.Where(rr => !rr.IsUsed);
        }

        var redemptions = await query
            .OrderByDescending(rr => rr.RedeemedAt)
            .Select(rr => new RewardRedemptionDto
            {
                Id = rr.Id,
                RewardId = rr.RewardId,
                RewardName = rr.Reward.Name,
                RewardDescription = rr.Reward.Description,
                CustomerLoyaltyId = rr.CustomerLoyaltyId,
                OrderId = rr.OrderId,
                OrderNumber = rr.Order != null ? rr.Order.OrderNumber : null,
                PointsUsed = rr.PointsUsed,
                RedeemedAt = rr.RedeemedAt,
                UsedAt = rr.UsedAt,
                IsUsed = rr.IsUsed,
                ExpiryDate = rr.ExpiryDate,
                IsExpired = rr.CheckExpiry()
            })
            .ToListAsync(cancellationToken);

        return redemptions;
    }
}
