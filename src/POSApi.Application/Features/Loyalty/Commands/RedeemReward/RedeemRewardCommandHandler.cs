using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.RedeemReward;

public class RedeemRewardCommandHandler : ICommandHandler<RedeemRewardCommand, Guid>
{
    private readonly PosDbContext _context;

    public RedeemRewardCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RedeemRewardCommand request, CancellationToken cancellationToken)
    {
        // Get customer loyalty
        var customerLoyalty = await _context.CustomerLoyalties
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            throw new InvalidOperationException($"Customer is not enrolled in the loyalty program");
        }

        // Get reward
        var reward = await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == request.RewardId, cancellationToken);

        if (reward == null)
        {
            throw new InvalidOperationException($"Reward with ID {request.RewardId} not found");
        }

        // Check customer's redemption count for this reward
        var customerRedemptionCount = await _context.RewardRedemptions
            .CountAsync(rr => rr.CustomerLoyaltyId == customerLoyalty.Id &&
                             rr.RewardId == request.RewardId,
                        cancellationToken);

        // Check if reward is available
        if (!reward.IsAvailableForRedemption(customerRedemptionCount))
        {
            throw new InvalidOperationException("Reward is not available for redemption");
        }

        // Check if customer has enough points
        if (customerLoyalty.CurrentPoints < reward.PointsCost)
        {
            throw new InvalidOperationException(
                $"Insufficient points. Required: {reward.PointsCost}, Available: {customerLoyalty.CurrentPoints}");
        }

        // Redeem points
        customerLoyalty.RedeemPoints(
            reward.PointsCost,
            $"Redeemed reward: {reward.Name}");

        // Mark reward as redeemed
        reward.Redeem();

        // Create redemption record
        var redemption = new RewardRedemption(
            request.RewardId,
            customerLoyalty.Id,
            reward.PointsCost);

        _context.RewardRedemptions.Add(redemption);
        await _context.SaveChangesAsync(cancellationToken);

        return redemption.Id;
    }
}
