using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Commands;

public record CalculatePerformanceMetricsCommand : IRequest<Guid>
{
    public Guid EmployeeProfileId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public MetricPeriodType PeriodType { get; init; }
}

public class CalculatePerformanceMetricsCommandHandler : IRequestHandler<CalculatePerformanceMetricsCommand, Guid>
{
    private readonly IPerformanceMetricRepository _metricRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICommissionTransactionRepository _commissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CalculatePerformanceMetricsCommandHandler(
        IPerformanceMetricRepository metricRepository,
        IShiftRepository shiftRepository,
        IOrderRepository orderRepository,
        ICommissionTransactionRepository commissionRepository,
        IUnitOfWork unitOfWork)
    {
        _metricRepository = metricRepository;
        _shiftRepository = shiftRepository;
        _orderRepository = orderRepository;
        _commissionRepository = commissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CalculatePerformanceMetricsCommand request, CancellationToken cancellationToken)
    {
        // Check if metric already exists for this period
        var existingMetric = await _metricRepository.GetByPeriodAsync(
            request.EmployeeProfileId, 
            request.PeriodStart, 
            request.PeriodEnd, 
            cancellationToken);

        PerformanceMetric metric;
        if (existingMetric != null)
        {
            metric = existingMetric;
        }
        else
        {
            metric = new PerformanceMetric(
                request.EmployeeProfileId,
                request.PeriodStart,
                request.PeriodEnd,
                request.PeriodType
            );
            await _metricRepository.AddAsync(metric, cancellationToken);
        }

        // Get shifts for the period
        var shifts = await _shiftRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);
        shifts = shifts.Where(s => s.EmployeeProfileId == request.EmployeeProfileId).ToList();

        // Calculate attendance metrics
        var scheduledShifts = shifts.Count;
        var completedShifts = shifts.Count(s => s.Status == ShiftStatus.Completed);
        var missedShifts = shifts.Count(s => s.Status == ShiftStatus.NoShow);
        var lateArrivals = shifts.Count(s => s.IsLate());
        var totalHours = shifts.Where(s => s.Status == ShiftStatus.Completed)
            .Sum(s => (decimal)(s.GetActualDuration()?.TotalHours ?? 0));
        var breakHours = shifts.Where(s => s.Status == ShiftStatus.Completed)
            .Sum(s => (decimal)s.ActualBreakMinutes / 60);

        metric.UpdateAttendanceMetrics(
            scheduledShifts,
            completedShifts,
            missedShifts,
            lateArrivals,
            totalHours,
            breakHours
        );

        // Get orders for the period (need to get employee's user ID)
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        orders = orders.Where(o => 
            o.OrderDate >= request.PeriodStart && 
            o.OrderDate <= request.PeriodEnd).ToList();

        // Calculate sales metrics
        var totalSales = orders.Sum(o => o.TotalAmount);
        var ordersProcessed = orders.Count;
        var itemsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);
        var refunds = orders.Where(o => o.Status == OrderStatus.Refunded);
        var refundsProcessed = refunds.Count();
        var refundAmount = refunds.Sum(o => o.TotalAmount);

        metric.UpdateSalesMetrics(
            totalSales,
            ordersProcessed,
            itemsSold,
            refundsProcessed,
            refundAmount
        );

        // Get commission earnings
        var commissionEarned = await _commissionRepository.GetTotalEarnedAsync(
            request.EmployeeProfileId,
            request.PeriodStart,
            request.PeriodEnd,
            cancellationToken
        );

        metric.UpdateCommissionEarned(commissionEarned);

        // Calculate final performance score
        metric.CalculatePerformanceScore();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return metric.Id;
    }
}
