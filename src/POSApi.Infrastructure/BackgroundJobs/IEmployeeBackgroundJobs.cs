namespace POSApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Interface for employee management background jobs
/// </summary>
public interface IEmployeeBackgroundJobs
{
    /// <summary>
    /// Calculate daily performance metrics for all active employees (runs daily at 3 AM UTC)
    /// </summary>
    Task CalculateDailyPerformanceMetricsAsync();

    /// <summary>
    /// Process weekly commission payments (runs weekly on Monday at 1 AM UTC)
    /// </summary>
    Task ProcessWeeklyCommissionsAsync();

    /// <summary>
    /// Generate monthly performance reports for all employees (runs monthly on 1st at 6 AM UTC)
    /// </summary>
    Task GenerateMonthlyPerformanceReportsAsync();

    /// <summary>
    /// Send shift reminder notifications (runs hourly)
    /// </summary>
    Task SendShiftRemindersAsync();
}
