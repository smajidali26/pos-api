using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Queries;

public record GetPerformanceComparisonQuery : IRequest<List<PerformanceMetricDto>>
{
    public List<Guid> EmployeeProfileIds { get; init; } = new();
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}

public class GetPerformanceComparisonQueryHandler : IRequestHandler<GetPerformanceComparisonQuery, List<PerformanceMetricDto>>
{
    private readonly IPerformanceMetricRepository _repository;

    public GetPerformanceComparisonQueryHandler(IPerformanceMetricRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PerformanceMetricDto>> Handle(GetPerformanceComparisonQuery request, CancellationToken cancellationToken)
    {
        var allMetrics = new List<PerformanceMetricDto>();

        foreach (var employeeId in request.EmployeeProfileIds)
        {
            var metric = await _repository.GetByPeriodAsync(
                employeeId, 
                request.PeriodStart, 
                request.PeriodEnd, 
                cancellationToken);

            if (metric != null)
            {
                allMetrics.Add(new PerformanceMetricDto
                {
                    Id = metric.Id,
                    EmployeeProfileId = metric.EmployeeProfileId,
                    EmployeeName = metric.EmployeeProfile?.User?.FullName ?? "Unknown",
                    PeriodStart = metric.PeriodStart,
                    PeriodEnd = metric.PeriodEnd,
                    PeriodType = metric.PeriodType.ToString(),
                    TotalSales = metric.TotalSales,
                    OrdersProcessed = metric.OrdersProcessed,
                    AverageOrderValue = metric.AverageOrderValue,
                    ItemsSold = metric.ItemsSold,
                    CommissionEarned = metric.CommissionEarned,
                    RefundsProcessed = metric.RefundsProcessed,
                    RefundAmount = metric.RefundAmount,
                    RefundRate = metric.RefundRate,
                    ScheduledShifts = metric.ScheduledShifts,
                    CompletedShifts = metric.CompletedShifts,
                    MissedShifts = metric.MissedShifts,
                    LateArrivals = metric.LateArrivals,
                    TotalHoursWorked = metric.TotalHoursWorked,
                    TotalBreakHours = metric.TotalBreakHours,
                    AttendanceRate = metric.GetAttendanceRate(),
                    SalesPerHour = metric.GetSalesPerHour(),
                    CustomerRatingCount = metric.CustomerRatingCount,
                    AverageCustomerRating = metric.AverageCustomerRating,
                    PerformanceScore = metric.PerformanceScore,
                    PerformanceGrade = metric.PerformanceGrade,
                    IsExcellentPerformer = metric.IsExcellentPerformer(),
                    NeedsImprovement = metric.NeedsImprovement(),
                    Notes = metric.Notes,
                    CreatedAt = metric.CreatedAt
                });
            }
        }

        return allMetrics.OrderByDescending(m => m.PerformanceScore).ToList();
    }
}
