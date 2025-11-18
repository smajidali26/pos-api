using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record MarkNoShowCommand(Guid ShiftId) : IRequest<Unit>;

public class MarkNoShowCommandHandler : IRequestHandler<MarkNoShowCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNoShowCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MarkNoShowCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.MarkNoShow();

        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
