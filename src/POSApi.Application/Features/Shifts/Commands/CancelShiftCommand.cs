using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record CancelShiftCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public string? Reason { get; init; }
}

public class CancelShiftCommandHandler : IRequestHandler<CancelShiftCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelShiftCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.Cancel(request.Reason);

        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
