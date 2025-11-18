using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetAvailableRewards;

public class GetAvailableRewardsQueryHandler : IQueryHandler<GetAvailableRewardsQuery, List<RewardDto>>
{
    private readonly PosDbContext _context;

    public GetAvailableRewardsQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<RewardDto>> Handle(GetAvailableRewardsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query = _context.Rewards
            .Include(r => r.Product)
            .Where(r => r.IsActive && r.ValidFrom <= now && r.ValidTo >= now)
            .AsQueryable();

        int? customerRedemptionCount = null;
        Guid? customerLoyaltyId = null;

        if (request.CustomerId.HasValue)
        {
            var customerLoyalty = await _context.CustomerLoyalties
                .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId.Value, cancellationToken);

            if (customerLoyalty != null)
            {
                customerLoyaltyId = customerLoyalty.Id;
            }
        }

        var rewards = await query
            .OrderBy(r => r.PointsCost)
            .ToListAsync(cancellationToken);

        var rewardDtos = new List<RewardDto>();

        foreach (var reward in rewards)
        {
            int customerRedemptions = 0;
            if (customerLoyaltyId.HasValue)
            {
                customerRedemptions = await _context.RewardRedemptions
                    .CountAsync(rr => rr.CustomerLoyaltyId == customerLoyaltyId.Value &&
                                     rr.RewardId == reward.Id,
                               cancellationToken);
            }

            rewardDtos.Add(new RewardDto
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description,
                PointsCost = reward.PointsCost,
                RewardType = reward.RewardType.ToString(),
                Value = reward.Value,
                ProductId = reward.ProductId,
                ProductName = reward.Product?.Name,
                ValidFrom = reward.ValidFrom,
                ValidTo = reward.ValidTo,
                MaxRedemptionsPerCustomer = reward.MaxRedemptionsPerCustomer,
                TotalRedemptionsAllowed = reward.TotalRedemptionsAllowed,
                CurrentRedemptions = reward.CurrentRedemptions,
                IsActive = reward.IsActive,
                IsAvailable = reward.IsAvailableForRedemption(customerRedemptions),
                CustomerRedemptions = customerLoyaltyId.HasValue ? customerRedemptions : null
            });
        }

        return rewardDtos;
    }
}
