using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetCustomerLoyalty;

public class GetCustomerLoyaltyQueryHandler : IQueryHandler<GetCustomerLoyaltyQuery, CustomerLoyaltyDto?>
{
    private readonly PosDbContext _context;

    public GetCustomerLoyaltyQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerLoyaltyDto?> Handle(GetCustomerLoyaltyQuery request, CancellationToken cancellationToken)
    {
        var loyalty = await _context.CustomerLoyalties
            .Include(cl => cl.Customer)
            .Include(cl => cl.CurrentTier)
            .Include(cl => cl.Transactions)
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (loyalty == null)
        {
            return null;
        }

        // Calculate points expiring in next 30 days
        var expiringTransactions = loyalty.GetExpiringTransactions(30);
        var pointsExpiring = expiringTransactions.Sum(t => t.GetAvailablePoints());

        return new CustomerLoyaltyDto
        {
            Id = loyalty.Id,
            CustomerId = loyalty.CustomerId,
            CustomerName = loyalty.Customer.FullName,
            CurrentPoints = loyalty.CurrentPoints,
            LifetimePoints = loyalty.LifetimePoints,
            LifetimeSpend = loyalty.LifetimeSpend,
            CurrentTierId = loyalty.CurrentTierId,
            CurrentTier = loyalty.CurrentTier != null ? new CustomerTierDto
            {
                Id = loyalty.CurrentTier.Id,
                Name = loyalty.CurrentTier.Name,
                MinSpend = loyalty.CurrentTier.MinSpend,
                MinPoints = loyalty.CurrentTier.MinPoints,
                BenefitMultiplier = loyalty.CurrentTier.BenefitMultiplier,
                DiscountPercentage = loyalty.CurrentTier.DiscountPercentage,
                Color = loyalty.CurrentTier.Color,
                SortOrder = loyalty.CurrentTier.SortOrder,
                CreatedAt = loyalty.CurrentTier.CreatedAt,
                UpdatedAt = loyalty.CurrentTier.UpdatedAt
            } : null,
            JoinDate = loyalty.JoinDate,
            LastActivityDate = loyalty.LastActivityDate,
            LastPointsExpiryCheckDate = loyalty.LastPointsExpiryCheckDate,
            PointsExpiringIn30Days = pointsExpiring
        };
    }
}
