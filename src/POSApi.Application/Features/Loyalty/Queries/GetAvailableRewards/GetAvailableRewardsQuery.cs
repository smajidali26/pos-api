using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Queries.GetAvailableRewards;

public class GetAvailableRewardsQuery : IQuery<List<RewardDto>>
{
    public Guid? CustomerId { get; set; }
}
