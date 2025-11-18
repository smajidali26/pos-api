using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record RecordBreakCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public int ActualBreakMinutes { get; init; }
}

public class RecordBreakCommandHandler : IRequestHandler<RecordBreakCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordBreakCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordBreakCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.UpdateBreakTime(request.ActualBreakMinutes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
