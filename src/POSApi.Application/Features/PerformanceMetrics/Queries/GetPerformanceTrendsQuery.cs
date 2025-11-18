using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Queries;

public record GetPerformanceTrendsQuery : IRequest<PerformanceTrendDto>
{
    public Guid EmployeeProfileId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public class GetPerformanceTrendsQueryHandler : IRequestHandler<GetPerformanceTrendsQuery, PerformanceTrendDto>
{
    private readonly IPerformanceMetricRepository _repository;

    public GetPerformanceTrendsQueryHandler(IPerformanceMetricRepository repository)
    {
        _repository = repository;
    }

    public async Task<PerformanceTrendDto> Handle(GetPerformanceTrendsQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _repository.GetByEmployeeIdAsync(request.EmployeeProfileId, cancellationToken);

        // Filter to date range
        metrics = metrics.Where(m => 
            m.PeriodStart >= request.StartDate && 
            m.PeriodEnd <= request.EndDate).ToList();

        var trend = new PerformanceTrendDto
        {
            EmployeeProfileId = request.EmployeeProfileId,
            EmployeeName = metrics.FirstOrDefault()?.EmployeeProfile?.User?.FullName ?? "Unknown",
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Metrics = metrics.Select(m => new PerformanceMetricDto
            {
                Id = m.Id,
                EmployeeProfileId = m.EmployeeProfileId,
                EmployeeName = m.EmployeeProfile?.User?.FullName ?? "Unknown",
                PeriodStart = m.PeriodStart,
                PeriodEnd = m.PeriodEnd,
                PeriodType = m.PeriodType.ToString(),
                TotalSales = m.TotalSales,
                OrdersProcessed = m.OrdersProcessed,
                AverageOrderValue = m.AverageOrderValue,
                ItemsSold = m.ItemsSold,
                CommissionEarned = m.CommissionEarned,
                RefundsProcessed = m.RefundsProcessed,
                RefundAmount = m.RefundAmount,
                RefundRate = m.RefundRate,
                ScheduledShifts = m.ScheduledShifts,
                CompletedShifts = m.CompletedShifts,
                MissedShifts = m.MissedShifts,
                LateArrivals = m.LateArrivals,
                TotalHoursWorked = m.TotalHoursWorked,
                TotalBreakHours = m.TotalBreakHours,
                AttendanceRate = m.GetAttendanceRate(),
                SalesPerHour = m.GetSalesPerHour(),
                CustomerRatingCount = m.CustomerRatingCount,
                AverageCustomerRating = m.AverageCustomerRating,
                PerformanceScore = m.PerformanceScore,
                PerformanceGrade = m.PerformanceGrade,
                IsExcellentPerformer = m.IsExcellentPerformer(),
                NeedsImprovement = m.NeedsImprovement(),
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            }).OrderBy(m => m.PeriodStart).ToList()
        };

        // Calculate trend statistics
        if (trend.Metrics.Any())
        {
            trend.AveragePerformanceScore = trend.Metrics.Average(m => m.PerformanceScore);
            trend.HighestScore = trend.Metrics.Max(m => m.PerformanceScore);
            trend.LowestScore = trend.Metrics.Min(m => m.PerformanceScore);
            trend.TotalSales = trend.Metrics.Sum(m => m.TotalSales);
            trend.TotalHoursWorked = trend.Metrics.Sum(m => m.TotalHoursWorked);
            trend.AverageSalesPerHour = trend.TotalHoursWorked > 0 
                ? trend.TotalSales / trend.TotalHoursWorked 
                : 0;
        }

        return trend;
    }
}

public class PerformanceTrendDto
{
    public Guid EmployeeProfileId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<PerformanceMetricDto> Metrics { get; set; } = new();
    
    // Trend Statistics
    public decimal AveragePerformanceScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal AverageSalesPerHour { get; set; }
}
