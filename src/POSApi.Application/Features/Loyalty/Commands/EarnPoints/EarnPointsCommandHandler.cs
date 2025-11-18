using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.EarnPoints;

public class EarnPointsCommandHandler : ICommandHandler<EarnPointsCommand, int>
{
    private readonly PosDbContext _context;

    public EarnPointsCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(EarnPointsCommand request, CancellationToken cancellationToken)
    {
        // Get the active loyalty program
        var program = await _context.LoyaltyPrograms
            .FirstOrDefaultAsync(p => p.IsActive, cancellationToken);

        if (program == null)
        {
            throw new InvalidOperationException("No active loyalty program found");
        }

        // Get customer loyalty profile
        var customerLoyalty = await _context.CustomerLoyalties
            .Include(cl => cl.CurrentTier)
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            throw new InvalidOperationException($"Customer is not enrolled in the loyalty program");
        }

        // Calculate base points
        int basePoints = program.CalculatePoints(request.OrderAmount);

        if (basePoints == 0)
        {
            return 0; // Order amount below minimum threshold
        }

        // Apply tier multiplier if applicable
        int finalPoints = basePoints;
        if (customerLoyalty.CurrentTier != null)
        {
            finalPoints = customerLoyalty.CurrentTier.ApplyPointsMultiplier(basePoints);
        }

        // Earn points
        customerLoyalty.EarnPoints(
            finalPoints,
            $"Points earned from order (${request.OrderAmount:F2})",
            program.PointsExpiryDays,
            request.OrderId,
            request.OrderAmount);

        // Check for tier promotion
        await CheckAndPromoteTierAsync(customerLoyalty, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return finalPoints;
    }

    private async Task CheckAndPromoteTierAsync(Domain.Entities.CustomerLoyalty customerLoyalty, CancellationToken cancellationToken)
    {
        var eligibleTier = await _context.CustomerTiers
            .Where(t => t.CheckEligibility(customerLoyalty.LifetimePoints, customerLoyalty.LifetimeSpend))
            .OrderByDescending(t => t.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (eligibleTier != null && eligibleTier.Id != customerLoyalty.CurrentTierId)
        {
            customerLoyalty.PromoteTier(eligibleTier.Id);
        }
    }
}
