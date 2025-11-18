using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Commands;

public record SwapShiftCommand : IRequest<Unit>
{
    public Guid ShiftId { get; init; }
    public Guid NewEmployeeProfileId { get; init; }
}

public class SwapShiftCommandHandler : IRequestHandler<SwapShiftCommand, Unit>
{
    private readonly IShiftRepository _repository;
    private readonly IEmployeeProfileRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SwapShiftCommandHandler(
        IShiftRepository repository,
        IEmployeeProfileRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SwapShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _repository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null)
            throw new KeyNotFoundException($"Shift with ID {request.ShiftId} not found");

        var newEmployee = await _employeeRepository.GetByIdAsync(request.NewEmployeeProfileId, cancellationToken);
        if (newEmployee == null)
            throw new KeyNotFoundException($"Employee with ID {request.NewEmployeeProfileId} not found");

        shift.SwapEmployee(request.NewEmployeeProfileId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
