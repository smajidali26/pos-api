using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Queries;

public record GetTopPerformersQuery : IRequest<List<PerformanceMetricDto>>
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int Count { get; init; } = 10;
}

public class GetTopPerformersQueryHandler : IRequestHandler<GetTopPerformersQuery, List<PerformanceMetricDto>>
{
    private readonly IPerformanceMetricRepository _repository;

    public GetTopPerformersQueryHandler(IPerformanceMetricRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PerformanceMetricDto>> Handle(GetTopPerformersQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _repository.GetTopPerformersAsync(
            request.PeriodStart, 
            request.PeriodEnd, 
            request.Count, 
            cancellationToken);

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
        }).ToList();
    }
}
