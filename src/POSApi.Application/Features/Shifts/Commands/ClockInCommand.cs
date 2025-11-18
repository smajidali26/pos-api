using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record ClockInCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public DateTime ClockInTime { get; init; }
    public string? Location { get; init; }
    public string? Device { get; init; }
}

public class ClockInCommandHandler : IRequestHandler<ClockInCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IShiftAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClockInCommandHandler(
        IShiftRepository repository,
        IShiftAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ClockInCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.ClockIn(request.ClockInTime);

        // Create attendance record
        var attendance = new ShiftAttendance(
            request.ShiftId,
            AttendanceEventType.ClockIn,
            request.ClockInTime,
            shift.IsLate() ? "Late arrival" : "On time",
            request.Location,
            request.Device
        );

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
