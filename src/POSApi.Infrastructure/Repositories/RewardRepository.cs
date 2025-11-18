using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class RewardRepository : Repository<Reward>, IRewardRepository
{
    public RewardRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<List<Reward>> GetActiveRewardsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.PointsCost)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reward>> GetByTypeAsync(RewardType rewardType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.RewardType == rewardType && r.IsActive)
            .OrderBy(r => r.PointsCost)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reward>> GetByPointRangeAsync(int minPoints, int maxPoints, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive &&
                       r.PointsCost >= minPoints &&
                       r.PointsCost <= maxPoints)
            .OrderBy(r => r.PointsCost)
            .ToListAsync(cancellationToken);
    }
}
