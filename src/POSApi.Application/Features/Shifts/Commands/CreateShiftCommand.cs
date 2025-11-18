using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record CreateShiftCommand : IRequest<Guid>
{
    public Guid EmployeeProfileId { get; init; }
    public DateTime ScheduledStartTime { get; init; }
    public DateTime ScheduledEndTime { get; init; }
    public int ScheduledBreakMinutes { get; init; } = 30;
    public Guid? StoreId { get; init; }
    public string? Notes { get; init; }
}

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, Guid>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShiftCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = new Shift(
            request.EmployeeProfileId,
            request.ScheduledStartTime,
            request.ScheduledEndTime,
            request.ScheduledBreakMinutes,
            request.StoreId,
            request.Notes
        );

        await _repository.AddAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shift.Id;
    }
}
