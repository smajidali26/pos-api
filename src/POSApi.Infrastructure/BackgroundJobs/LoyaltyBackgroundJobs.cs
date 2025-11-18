using Microsoft.Extensions.Logging;
using POSApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace POSApi.Infrastructure.BackgroundJobs;

public class LoyaltyBackgroundJobs : ILoyaltyBackgroundJobs
{
    private readonly PosDbContext _context;
    private readonly ILogger<LoyaltyBackgroundJobs> _logger;

    public LoyaltyBackgroundJobs(
        PosDbContext context,
        ILogger<LoyaltyBackgroundJobs> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExpirePointsAsync()
    {
        _logger.LogInformation("Starting points expiry check at {Time}", DateTime.UtcNow);

        try
        {
            var today = DateTime.UtcNow.Date;

            // Find all transactions with points that expire today
            var expiringTransactions = await _context.LoyaltyTransactions
                .Include(t => t.CustomerLoyalty)
                .Where(t => t.TransactionType == Domain.Entities.LoyaltyTransactionType.Earned &&
                           t.ExpiryDate.HasValue &&
                           t.ExpiryDate.Value.Date == today &&
                           t.PointsEarned > 0)
                .ToListAsync();

            _logger.LogInformation("Found {Count} transactions with expiring points", expiringTransactions.Count);

            var totalPointsExpired = 0;
            var customersAffected = 0;

            foreach (var transaction in expiringTransactions)
            {
                try
                {
                    var availablePoints = transaction.GetAvailablePoints();

                    if (availablePoints > 0)
                    {
                        // Create expiry transaction
                        var expiryTransaction = new Domain.Entities.LoyaltyTransaction(
                            transaction.CustomerLoyaltyId,
                            0, // pointsEarned
                            availablePoints, // pointsExpired
                            $"Points expired from transaction on {transaction.TransactionDate:yyyy-MM-dd}",
                            Domain.Entities.LoyaltyTransactionType.Expired
                        );

                        _context.LoyaltyTransactions.Add(expiryTransaction);

                        // Update customer's current points
                        var customerLoyalty = transaction.CustomerLoyalty;
                        customerLoyalty.CurrentPoints -= availablePoints;

                        totalPointsExpired += availablePoints;
                        customersAffected++;

                        _logger.LogDebug("Expired {Points} points for customer {CustomerId}",
                            availablePoints, transaction.CustomerLoyalty.CustomerId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error expiring points for transaction {TransactionId}",
                        transaction.Id);
                }
            }

            if (totalPointsExpired > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Points expiry completed. Total points expired: {Points}, Customers affected: {Customers}",
                totalPointsExpired, customersAffected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during points expiry check");
            throw;
        }
    }

    public async Task SendPointsExpiryNotificationsAsync()
    {
        _logger.LogInformation("Starting points expiry notifications at {Time}", DateTime.UtcNow);

        try
        {
            var notificationDate = DateTime.UtcNow.AddDays(30).Date;

            // Find customers with points expiring in the next 30 days
            var customersWithExpiringPoints = await _context.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Include(cl => cl.Transactions)
                .Where(cl => cl.Transactions.Any(t =>
                    t.TransactionType == Domain.Entities.LoyaltyTransactionType.Earned &&
                    t.ExpiryDate.HasValue &&
                    t.ExpiryDate.Value.Date <= notificationDate &&
                    t.ExpiryDate.Value.Date > DateTime.UtcNow.Date))
                .ToListAsync();

            _logger.LogInformation("Found {Count} customers with points expiring in 30 days",
                customersWithExpiringPoints.Count);

            var notificationsSent = 0;

            foreach (var customerLoyalty in customersWithExpiringPoints)
            {
                try
                {
                    // Calculate total expiring points
                    var expiringPoints = customerLoyalty.Transactions
                        .Where(t => t.TransactionType == Domain.Entities.LoyaltyTransactionType.Earned &&
                                   t.ExpiryDate.HasValue &&
                                   t.ExpiryDate.Value.Date <= notificationDate &&
                                   t.ExpiryDate.Value.Date > DateTime.UtcNow.Date)
                        .Sum(t => t.GetAvailablePoints());

                    if (expiringPoints > 0)
                    {
                        // TODO: Implement email/SMS notification service
                        // For now, just log the notification
                        _logger.LogInformation(
                            "Notification: Customer {CustomerId} ({Email}) has {Points} points expiring within 30 days",
                            customerLoyalty.CustomerId,
                            customerLoyalty.Customer.Email,
                            expiringPoints);

                        notificationsSent++;

                        // You can add email sending logic here:
                        // await _emailService.SendAsync(
                        //     customerLoyalty.Customer.Email,
                        //     "Points Expiring Soon",
                        //     $"You have {expiringPoints} points expiring within 30 days. Use them before they expire!"
                        // );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification to customer {CustomerId}",
                        customerLoyalty.CustomerId);
                }
            }

            _logger.LogInformation("Expiry notifications completed. Sent: {Count}", notificationsSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during points expiry notifications");
            throw;
        }
    }

    public async Task UpdateCustomerTiersAsync()
    {
        _logger.LogInformation("Starting customer tier updates at {Time}", DateTime.UtcNow);

        try
        {
            // Get all active customer loyalty accounts
            var customerLoyalties = await _context.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Include(cl => cl.CurrentTier)
                .ToListAsync();

            // Get all tiers ordered by requirements
            var tiers = await _context.CustomerTiers
                .OrderByDescending(t => t.MinPoints)
                .ToListAsync();

            _logger.LogInformation("Checking tiers for {Count} customers", customerLoyalties.Count);

            var tiersUpdated = 0;
            var tiersPromoted = 0;
            var tiersDemoted = 0;

            foreach (var customerLoyalty in customerLoyalties)
            {
                try
                {
                    // Find the highest tier the customer qualifies for
                    var eligibleTier = tiers
                        .FirstOrDefault(t => t.CheckEligibility(
                            customerLoyalty.LifetimePoints,
                            customerLoyalty.LifetimeSpend));

                    if (eligibleTier != null && eligibleTier.Id != customerLoyalty.CurrentTierId)
                    {
                        var oldTierName = customerLoyalty.CurrentTier?.Name ?? "None";
                        customerLoyalty.PromoteTier(eligibleTier.Id);

                        tiersUpdated++;
                        if (eligibleTier.SortOrder > (customerLoyalty.CurrentTier?.SortOrder ?? 0))
                        {
                            tiersPromoted++;
                            _logger.LogInformation(
                                "Customer {CustomerId} promoted from {OldTier} to {NewTier}",
                                customerLoyalty.CustomerId, oldTierName, eligibleTier.Name);
                        }
                        else
                        {
                            tiersDemoted++;
                            _logger.LogInformation(
                                "Customer {CustomerId} moved from {OldTier} to {NewTier}",
                                customerLoyalty.CustomerId, oldTierName, eligibleTier.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating tier for customer {CustomerId}",
                        customerLoyalty.CustomerId);
                }
            }

            if (tiersUpdated > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Tier updates completed. Total: {Total}, Promoted: {Promoted}, Other: {Other}",
                tiersUpdated, tiersPromoted, tiersDemoted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer tier updates");
            throw;
        }
    }

    public async Task GenerateMonthlyLoyaltyReportAsync()
    {
        _logger.LogInformation("Starting monthly loyalty report generation at {Time}", DateTime.UtcNow);

        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-1);
            var endDate = DateTime.UtcNow;

            // Get statistics for the month
            var totalMembers = await _context.CustomerLoyalties.CountAsync();
            var newMembers = await _context.CustomerLoyalties
                .Where(cl => cl.JoinDate >= startDate && cl.JoinDate < endDate)
                .CountAsync();

            var pointsEarned = await _context.LoyaltyTransactions
                .Where(t => t.TransactionDate >= startDate &&
                           t.TransactionDate < endDate &&
                           t.TransactionType == Domain.Entities.LoyaltyTransactionType.Earned)
                .SumAsync(t => t.PointsEarned);

            var pointsRedeemed = await _context.LoyaltyTransactions
                .Where(t => t.TransactionDate >= startDate &&
                           t.TransactionDate < endDate &&
                           t.TransactionType == Domain.Entities.LoyaltyTransactionType.Redeemed)
                .SumAsync(t => t.PointsRedeemed);

            var pointsExpired = await _context.LoyaltyTransactions
                .Where(t => t.TransactionDate >= startDate &&
                           t.TransactionDate < endDate &&
                           t.TransactionType == Domain.Entities.LoyaltyTransactionType.Expired)
                .SumAsync(t => t.PointsRedeemed);

            var activeMembers = await _context.CustomerLoyalties
                .Where(cl => cl.LastActivityDate >= startDate && cl.LastActivityDate < endDate)
                .CountAsync();

            // Distribution by tier
            var tierDistribution = await _context.CustomerLoyalties
                .Include(cl => cl.CurrentTier)
                .GroupBy(cl => cl.CurrentTier!.Name)
                .Select(g => new { Tier = g.Key, Count = g.Count() })
                .ToListAsync();

            // Log the report
            _logger.LogInformation("=== Monthly Loyalty Report ({StartDate:yyyy-MM} to {EndDate:yyyy-MM}) ===",
                startDate, endDate);
            _logger.LogInformation("Total Members: {Total}", totalMembers);
            _logger.LogInformation("New Members: {New}", newMembers);
            _logger.LogInformation("Active Members: {Active}", activeMembers);
            _logger.LogInformation("Points Earned: {Earned}", pointsEarned);
            _logger.LogInformation("Points Redeemed: {Redeemed}", pointsRedeemed);
            _logger.LogInformation("Points Expired: {Expired}", pointsExpired);
            _logger.LogInformation("Net Points Change: {Net}", pointsEarned - pointsRedeemed - pointsExpired);
            _logger.LogInformation("Tier Distribution:");
            foreach (var tier in tierDistribution)
            {
                _logger.LogInformation("  {Tier}: {Count}", tier.Tier, tier.Count);
            }
            _logger.LogInformation("=== End of Report ===");

            // TODO: Save report to database or send via email
            // You could create a MonthlyReport entity and save the data

            _logger.LogInformation("Monthly loyalty report generation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during monthly loyalty report generation");
            throw;
        }
    }
}
