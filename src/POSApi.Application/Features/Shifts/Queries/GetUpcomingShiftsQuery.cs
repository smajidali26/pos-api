using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Queries;

public record GetUpcomingShiftsQuery : IRequest<List<ShiftDto>>
{
    public Guid EmployeeProfileId { get; init; }
    public int DaysAhead { get; init; } = 7;
}

public class GetUpcomingShiftsQueryHandler : IRequestHandler<GetUpcomingShiftsQuery, List<ShiftDto>>
{
    private readonly IShiftRepository _repository;

    public GetUpcomingShiftsQueryHandler(IShiftRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ShiftDto>> Handle(GetUpcomingShiftsQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _repository.GetUpcomingShiftsAsync(request.EmployeeProfileId, cancellationToken);

        // Filter to only include shifts within the specified days ahead
        var cutoffDate = DateTime.UtcNow.AddDays(request.DaysAhead);
        shifts = shifts.Where(s => s.ScheduledStartTime >= DateTime.UtcNow && s.ScheduledStartTime <= cutoffDate).ToList();

        return shifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            EmployeeProfileId = s.EmployeeProfileId,
            EmployeeName = s.EmployeeProfile?.User?.FullName ?? "Unknown",
            StoreId = s.StoreId,
            StoreName = s.Store?.Name,
            ScheduledStartTime = s.ScheduledStartTime,
            ScheduledEndTime = s.ScheduledEndTime,
            ActualStartTime = s.ActualStartTime,
            ActualEndTime = s.ActualEndTime,
            Status = s.Status.ToString(),
            ScheduledBreakMinutes = s.ScheduledBreakMinutes,
            ActualBreakMinutes = s.ActualBreakMinutes,
            TotalSales = s.TotalSales,
            OrdersProcessed = s.OrdersProcessed,
            ScheduledHours = s.GetScheduledDuration().TotalHours,
            ActualHours = s.GetActualDuration()?.TotalHours,
            IsLate = s.IsLate(),
            Notes = s.Notes,
            CreatedAt = s.CreatedAt
        }).OrderBy(s => s.ScheduledStartTime).ToList();
    }
}
