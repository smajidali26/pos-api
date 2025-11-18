using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Queries;

public record GetEmployeePerformanceQuery : IRequest<List<PerformanceMetricDto>>
{
    public Guid EmployeeProfileId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetEmployeePerformanceQueryHandler : IRequestHandler<GetEmployeePerformanceQuery, List<PerformanceMetricDto>>
{
    private readonly IPerformanceMetricRepository _repository;

    public GetEmployeePerformanceQueryHandler(IPerformanceMetricRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PerformanceMetricDto>> Handle(GetEmployeePerformanceQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _repository.GetByEmployeeIdAsync(request.EmployeeProfileId, cancellationToken);

        // Apply date filters if provided
        if (request.StartDate.HasValue)
            metrics = metrics.Where(m => m.PeriodStart >= request.StartDate.Value).ToList();

        if (request.EndDate.HasValue)
            metrics = metrics.Where(m => m.PeriodEnd <= request.EndDate.Value).ToList();

        return metrics.Select(m => new PerformanceMetricDto
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
        }).OrderByDescending(m => m.PeriodStart).ToList();
    }
}

public class PerformanceMetricDto
{
    public Guid Id { get; set; }
    public Guid EmployeeProfileId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodType { get; set; } = string.Empty;

    // Sales Metrics
    public decimal TotalSales { get; set; }
    public int OrdersProcessed { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ItemsSold { get; set; }

    // Performance Metrics
    public decimal? CommissionEarned { get; set; }
    public int RefundsProcessed { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RefundRate { get; set; }

    // Attendance Metrics
    public int ScheduledShifts { get; set; }
    public int CompletedShifts { get; set; }
    public int MissedShifts { get; set; }
    public int LateArrivals { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal TotalBreakHours { get; set; }
    public decimal AttendanceRate { get; set; }
    public decimal SalesPerHour { get; set; }

    // Customer Satisfaction
    public int? CustomerRatingCount { get; set; }
    public decimal? AverageCustomerRating { get; set; }

    // Calculated Score
    public decimal PerformanceScore { get; set; }
    public string? PerformanceGrade { get; set; }
    public bool IsExcellentPerformer { get; set; }
    public bool NeedsImprovement { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
