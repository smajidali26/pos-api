using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Queries;

public record GetActiveShiftsQuery : IRequest<List<ShiftDto>>
{
    public Guid? StoreId { get; init; }
}

public class GetActiveShiftsQueryHandler : IRequestHandler<GetActiveShiftsQuery, List<ShiftDto>>
{
    private readonly IShiftRepository _repository;

    public GetActiveShiftsQueryHandler(IShiftRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ShiftDto>> Handle(GetActiveShiftsQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _repository.GetActiveShiftsAsync(cancellationToken);

        // Filter by store if provided
        if (request.StoreId.HasValue)
            shifts = shifts.Where(s => s.StoreId == request.StoreId.Value).ToList();

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
