namespace POSApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Interface for loyalty program background jobs
/// </summary>
public interface ILoyaltyBackgroundJobs
{
    /// <summary>
    /// Check and expire points that have reached their expiry date (runs daily at 1 AM)
    /// </summary>
    Task ExpirePointsAsync();

    /// <summary>
    /// Send notifications to customers with points expiring within 30 days (runs weekly on Monday at 9 AM)
    /// </summary>
    Task SendPointsExpiryNotificationsAsync();

    /// <summary>
    /// Check and update customer tiers based on lifetime points/spend (runs daily at 5 AM)
    /// </summary>
    Task UpdateCustomerTiersAsync();

    /// <summary>
    /// Generate monthly loyalty program reports (runs monthly on 1st at 6 AM)
    /// </summary>
    Task GenerateMonthlyLoyaltyReportAsync();
}
