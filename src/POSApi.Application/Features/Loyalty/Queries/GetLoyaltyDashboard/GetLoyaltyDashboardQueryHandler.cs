using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using System.Globalization;

namespace POSApi.Application.Features.Loyalty.Queries.GetLoyaltyDashboard;

public class GetLoyaltyDashboardQueryHandler : IQueryHandler<GetLoyaltyDashboardQuery, LoyaltyDashboardDto>
{
    private readonly PosDbContext _context;

    public GetLoyaltyDashboardQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<LoyaltyDashboardDto> Handle(GetLoyaltyDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        // Total active members
        var totalActiveMembers = await _context.CustomerLoyalties
            .CountAsync(cancellationToken);

        // New members this month
        var newMembersThisMonth = await _context.CustomerLoyalties
            .CountAsync(cl => cl.JoinDate >= firstDayOfMonth, cancellationToken);

        // Total points issued and redeemed
        var totalPointsIssued = await _context.LoyaltyTransactions
            .Where(t => t.TransactionType == LoyaltyTransactionType.Earned ||
                       t.TransactionType == LoyaltyTransactionType.Bonus)
            .SumAsync(t => t.PointsEarned, cancellationToken);

        var totalPointsRedeemed = await _context.LoyaltyTransactions
            .Where(t => t.TransactionType == LoyaltyTransactionType.Redeemed)
            .SumAsync(t => t.PointsRedeemed, cancellationToken);

        var pointsOutstanding = await _context.CustomerLoyalties
            .SumAsync(cl => cl.CurrentPoints, cancellationToken);

        var totalLifetimeSpend = await _context.CustomerLoyalties
            .SumAsync(cl => cl.LifetimeSpend, cancellationToken);

        var averagePointsPerMember = totalActiveMembers > 0
            ? (decimal)pointsOutstanding / totalActiveMembers
            : 0;

        var redemptionRate = totalPointsIssued > 0
            ? (decimal)totalPointsRedeemed / totalPointsIssued * 100
            : 0;

        // Tier distribution
        var tierDistribution = await _context.CustomerLoyalties
            .Include(cl => cl.CurrentTier)
            .Where(cl => cl.CurrentTierId != null)
            .GroupBy(cl => new { cl.CurrentTierId, cl.CurrentTier!.Name, cl.CurrentTier.Color })
            .Select(g => new TierDistributionDto
            {
                TierId = g.Key.CurrentTierId!.Value,
                TierName = g.Key.Name,
                Color = g.Key.Color,
                MemberCount = g.Count(),
                Percentage = totalActiveMembers > 0 ? (decimal)g.Count() / totalActiveMembers * 100 : 0
            })
            .ToListAsync(cancellationToken);

        // Top customers
        var topCustomers = await _context.CustomerLoyalties
            .Include(cl => cl.Customer)
            .Include(cl => cl.CurrentTier)
            .OrderByDescending(cl => cl.LifetimePoints)
            .Take(request.TopCustomersCount)
            .Select(cl => new TopLoyaltyCustomerDto
            {
                CustomerId = cl.CustomerId,
                CustomerName = cl.Customer.FullName,
                CurrentPoints = cl.CurrentPoints,
                LifetimePoints = cl.LifetimePoints,
                LifetimeSpend = cl.LifetimeSpend,
                TierName = cl.CurrentTier != null ? cl.CurrentTier.Name : "No Tier"
            })
            .ToListAsync(cancellationToken);

        // Monthly activity
        var startDate = now.AddMonths(-request.MonthsBack);
        var monthlyActivity = await _context.LoyaltyTransactions
            .Where(t => t.TransactionDate >= startDate)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                PointsEarned = g.Where(t => t.TransactionType == LoyaltyTransactionType.Earned ||
                                           t.TransactionType == LoyaltyTransactionType.Bonus)
                                .Sum(t => t.PointsEarned),
                PointsRedeemed = g.Where(t => t.TransactionType == LoyaltyTransactionType.Redeemed)
                                  .Sum(t => t.PointsRedeemed)
            })
            .ToListAsync(cancellationToken);

        var monthlyActivityDtos = monthlyActivity
            .Select(m => new MonthlyPointsActivityDto
            {
                Year = m.Year,
                Month = m.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month),
                PointsEarned = m.PointsEarned,
                PointsRedeemed = m.PointsRedeemed,
                NetPoints = m.PointsEarned - m.PointsRedeemed
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return new LoyaltyDashboardDto
        {
            TotalActiveMembers = totalActiveMembers,
            NewMembersThisMonth = newMembersThisMonth,
            TotalPointsIssued = totalPointsIssued,
            TotalPointsRedeemed = totalPointsRedeemed,
            PointsOutstanding = pointsOutstanding,
            TotalLifetimeSpend = totalLifetimeSpend,
            AveragePointsPerMember = averagePointsPerMember,
            RedemptionRate = redemptionRate,
            TierDistribution = tierDistribution,
            TopCustomers = topCustomers,
            MonthlyActivity = monthlyActivityDtos
        };
    }
}
