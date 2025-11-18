using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Tracks employee performance metrics for a specific period
/// </summary>
public class PerformanceMetric : BaseEntity
{
    public Guid EmployeeProfileId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public MetricPeriodType PeriodType { get; private set; }

    // Sales Metrics
    public decimal TotalSales { get; private set; }
    public int OrdersProcessed { get; private set; }
    public decimal AverageOrderValue { get; private set; }
    public int ItemsSold { get; private set; }

    // Performance Metrics
    public decimal? CommissionEarned { get; private set; }
    public int RefundsProcessed { get; private set; }
    public decimal RefundAmount { get; private set; }
    public decimal RefundRate { get; private set; } // Percentage

    // Attendance Metrics
    public int ScheduledShifts { get; private set; }
    public int CompletedShifts { get; private set; }
    public int MissedShifts { get; private set; }
    public int LateArrivals { get; private set; }
    public decimal TotalHoursWorked { get; private set; }
    public decimal TotalBreakHours { get; private set; }

    // Customer Satisfaction (if available)
    public int? CustomerRatingCount { get; private set; }
    public decimal? AverageCustomerRating { get; private set; }

    // Calculated Performance Score
    public decimal PerformanceScore { get; private set; }
    public string? PerformanceGrade { get; private set; } // A, B, C, D, F
    public string? Notes { get; private set; }

    // Navigation Properties
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    // Constructor for EF Core
    private PerformanceMetric() { }

    public PerformanceMetric(
        Guid employeeProfileId,
        DateTime periodStart,
        DateTime periodEnd,
        MetricPeriodType periodType)
    {
        if (periodEnd <= periodStart)
            throw new ArgumentException("Period end must be after period start");

        EmployeeProfileId = employeeProfileId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        PeriodType = periodType;
    }

    public void UpdateSalesMetrics(
        decimal totalSales,
        int ordersProcessed,
        int itemsSold,
        int refundsProcessed,
        decimal refundAmount)
    {
        TotalSales = totalSales;
        OrdersProcessed = ordersProcessed;
        ItemsSold = itemsSold;
        RefundsProcessed = refundsProcessed;
        RefundAmount = refundAmount;

        // Calculate derived metrics
        AverageOrderValue = ordersProcessed > 0 ? totalSales / ordersProcessed : 0;
        RefundRate = totalSales > 0 ? (refundAmount / totalSales) * 100 : 0;
    }

    public void UpdateAttendanceMetrics(
        int scheduledShifts,
        int completedShifts,
        int missedShifts,
        int lateArrivals,
        decimal totalHoursWorked,
        decimal totalBreakHours)
    {
        ScheduledShifts = scheduledShifts;
        CompletedShifts = completedShifts;
        MissedShifts = missedShifts;
        LateArrivals = lateArrivals;
        TotalHoursWorked = totalHoursWorked;
        TotalBreakHours = totalBreakHours;
    }

    public void UpdateCommissionEarned(decimal commissionEarned)
    {
        CommissionEarned = commissionEarned;
    }

    public void UpdateCustomerSatisfaction(int ratingCount, decimal averageRating)
    {
        CustomerRatingCount = ratingCount;
        AverageCustomerRating = averageRating;
    }

    public void CalculatePerformanceScore()
    {
        /*
         * Performance Score Calculation (out of 100):
         * - Sales Performance: 40 points
         * - Attendance: 30 points
         * - Customer Satisfaction: 20 points
         * - Low Refund Rate: 10 points
         */

        decimal score = 0;

        // Sales Performance (40 points max)
        // Based on sales per hour worked
        if (TotalHoursWorked > 0)
        {
            var salesPerHour = TotalSales / TotalHoursWorked;
            // Assuming $500/hour is excellent
            score += Math.Min(40, (salesPerHour / 500) * 40);
        }

        // Attendance (30 points max)
        if (ScheduledShifts > 0)
        {
            var attendanceRate = (decimal)CompletedShifts / ScheduledShifts;
            score += attendanceRate * 30;

            // Penalty for late arrivals
            var lateRate = (decimal)LateArrivals / ScheduledShifts;
            score -= lateRate * 10; // Up to -10 points
        }

        // Customer Satisfaction (20 points max)
        if (AverageCustomerRating.HasValue)
        {
            // Assuming rating is out of 5
            score += (AverageCustomerRating.Value / 5) * 20;
        }
        else
        {
            // No rating data, give neutral score
            score += 10;
        }

        // Low Refund Rate (10 points max)
        if (TotalSales > 0)
        {
            var refundPenalty = RefundRate * 2; // 2 points per 1% refund rate
            score += Math.Max(0, 10 - refundPenalty);
        }
        else
        {
            score += 10;
        }

        // Ensure score is between 0 and 100
        PerformanceScore = Math.Max(0, Math.Min(100, score));

        // Assign grade
        PerformanceGrade = PerformanceScore switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
    }

    public decimal GetAttendanceRate()
    {
        if (ScheduledShifts == 0)
            return 0;

        return ((decimal)CompletedShifts / ScheduledShifts) * 100;
    }

    public decimal GetSalesPerHour()
    {
        if (TotalHoursWorked == 0)
            return 0;

        return TotalSales / TotalHoursWorked;
    }

    public bool IsExcellentPerformer()
    {
        return PerformanceScore >= 90;
    }

    public bool NeedsImprovement()
    {
        return PerformanceScore < 70;
    }
}

public enum MetricPeriodType
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    Yearly = 4
}
