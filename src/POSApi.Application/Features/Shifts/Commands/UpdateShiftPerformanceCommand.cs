using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record UpdateShiftPerformanceCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public decimal TotalSales { get; init; }
    public int OrdersProcessed { get; init; }
}

public class UpdateShiftPerformanceCommandHandler : IRequestHandler<UpdateShiftPerformanceCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShiftPerformanceCommandHandler(
        IShiftRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateShiftPerformanceCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        shift.UpdatePerformance(request.TotalSales, request.OrdersProcessed);

        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
