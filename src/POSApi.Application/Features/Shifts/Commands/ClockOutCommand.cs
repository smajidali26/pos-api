using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record ClockOutCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public DateTime ClockOutTime { get; init; }
    public int ActualBreakMinutes { get; init; }
    public string? Location { get; init; }
    public string? Device { get; init; }
}

public class ClockOutCommandHandler : IRequestHandler<ClockOutCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IShiftAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClockOutCommandHandler(
        IShiftRepository repository,
        IShiftAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ClockOutCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.ClockOut(request.ClockOutTime, request.ActualBreakMinutes);

        // Create attendance record
        var attendance = new ShiftAttendance(
            request.ShiftId,
            AttendanceEventType.ClockOut,
            request.ClockOutTime,
            $"Worked {shift.GetActualDuration()?.TotalHours:F2} hours",
            request.Location,
            request.Device
        );

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
