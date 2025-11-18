using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record UpdateShiftCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public DateTime ScheduledStartTime { get; init; }
    public DateTime ScheduledEndTime { get; init; }
    public int ScheduledBreakMinutes { get; init; }
    public string? Notes { get; init; }
}

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShiftCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.UpdateSchedule(
            request.ScheduledStartTime,
            request.ScheduledEndTime,
            request.ScheduledBreakMinutes,
            request.Notes
        );

        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
