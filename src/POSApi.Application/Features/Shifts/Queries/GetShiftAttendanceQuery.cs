using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Queries;

public record GetShiftAttendanceQuery : IRequest<ShiftWithAttendanceDto>
{
    public Guid ShiftId { get; init; }
}

public class GetShiftAttendanceQueryHandler : IRequestHandler<GetShiftAttendanceQuery, ShiftWithAttendanceDto>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IShiftAttendanceRepository _attendanceRepository;

    public GetShiftAttendanceQueryHandler(
        IShiftRepository shiftRepository,
        IShiftAttendanceRepository attendanceRepository)
    {
        _shiftRepository = shiftRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<ShiftWithAttendanceDto> Handle(GetShiftAttendanceQuery request, CancellationToken cancellationToken)
    {
        var shift = await _shiftRepository.GetByIdWithAttendanceAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        var attendances = await _attendanceRepository.GetByShiftIdAsync(request.ShiftId, cancellationToken);

        return new ShiftWithAttendanceDto
        {
            Id = shift.Id,
            EmployeeProfileId = shift.EmployeeProfileId,
            EmployeeName = shift.EmployeeProfile?.User?.FullName ?? "Unknown",
            StoreId = shift.StoreId,
            StoreName = shift.Store?.Name,
            ScheduledStartTime = shift.ScheduledStartTime,
            ScheduledEndTime = shift.ScheduledEndTime,
            ActualStartTime = shift.ActualStartTime,
            ActualEndTime = shift.ActualEndTime,
            Status = shift.Status.ToString(),
            ScheduledBreakMinutes = shift.ScheduledBreakMinutes,
            ActualBreakMinutes = shift.ActualBreakMinutes,
            TotalSales = shift.TotalSales,
            OrdersProcessed = shift.OrdersProcessed,
            ScheduledHours = shift.GetScheduledDuration().TotalHours,
            ActualHours = shift.GetActualDuration()?.TotalHours,
            IsLate = shift.IsLate(),
            Notes = shift.Notes,
            CreatedAt = shift.CreatedAt,
            Attendances = attendances.Select(a => new AttendanceEventDto
            {
                Id = a.Id,
                EventType = a.EventType.ToString(),
                Timestamp = a.Timestamp,
                Notes = a.Notes,
                Location = a.Location,
                Device = a.Device
            }).OrderBy(a => a.Timestamp).ToList()
        };
    }
}

public class ShiftWithAttendanceDto : ShiftDto
{
    public List<AttendanceEventDto> Attendances { get; set; } = new();
}

public class AttendanceEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? Device { get; set; }
}
