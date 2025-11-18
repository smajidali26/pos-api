namespace POSApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Interface for analytics background jobs
/// </summary>
public interface IAnalyticsBackgroundJobs
{
    /// <summary>
    /// Generate sales forecasts for all products (runs daily at 2 AM)
    /// </summary>
    Task GenerateDailySalesForecastsAsync();

    /// <summary>
    /// Recalculate ABC classification for all products (runs monthly on 1st at 3 AM)
    /// </summary>
    Task RecalculateABCClassificationAsync();

    /// <summary>
    /// Calculate inventory turnover for all products (runs monthly on 1st at 4 AM)
    /// </summary>
    Task CalculateInventoryTurnoverAsync();

    /// <summary>
    /// Clean up old forecast data (runs weekly on Sunday at 1 AM)
    /// </summary>
    Task CleanupOldForecastsAsync();
}
