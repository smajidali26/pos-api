using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IRewardRepository : IRepository<Reward>
{
    Task<List<Reward>> GetActiveRewardsAsync(CancellationToken cancellationToken = default);
    Task<List<Reward>> GetByTypeAsync(RewardType rewardType, CancellationToken cancellationToken = default);
    Task<List<Reward>> GetByPointRangeAsync(int minPoints, int maxPoints, CancellationToken cancellationToken = default);
}
