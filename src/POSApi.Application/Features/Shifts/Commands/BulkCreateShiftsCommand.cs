using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record BulkCreateShiftsCommand : IRequest<List<Guid>>
{
    public List<ShiftScheduleDto> Shifts { get; init; } = new();
}

public class ShiftScheduleDto
{
    public Guid EmployeeProfileId { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int ScheduledBreakMinutes { get; set; } = 30;
    public Guid? StoreId { get; set; }
    public string? Notes { get; set; }
}

public class BulkCreateShiftsCommandHandler : IRequestHandler<BulkCreateShiftsCommand, List<Guid>>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkCreateShiftsCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Guid>> Handle(BulkCreateShiftsCommand request, CancellationToken cancellationToken)
    {
        var shiftIds = new List<Guid>();

        foreach (var shiftDto in request.Shifts)
        {
            var shift = new Shift(
                shiftDto.EmployeeProfileId,
                shiftDto.ScheduledStartTime,
                shiftDto.ScheduledEndTime,
                shiftDto.ScheduledBreakMinutes,
                shiftDto.StoreId,
                shiftDto.Notes
            );

            await _repository.AddAsync(shift, cancellationToken);
            shiftIds.Add(shift.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shiftIds;
    }
}
