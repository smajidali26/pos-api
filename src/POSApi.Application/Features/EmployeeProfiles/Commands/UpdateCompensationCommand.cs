using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.EmployeeProfiles.Commands;

public record UpdateCompensationCommand : IRequest<Unit>
{
    public Guid EmployeeProfileId { get; init; }
    public decimal HourlyRate { get; init; }
    public bool IsEligibleForCommission { get; init; }
}

public class UpdateCompensationCommandHandler : IRequestHandler<UpdateCompensationCommand, Unit>
{
    private readonly IEmployeeProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompensationCommandHandler(
        IEmployeeProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCompensationCommand request, CancellationToken cancellationToken)
    {
        var employeeProfile = await _repository.GetByIdAsync(request.EmployeeProfileId, cancellationToken);
        if (employeeProfile == null)
            throw new KeyNotFoundException($"Employee profile with ID {request.EmployeeProfileId} not found");

        employeeProfile.UpdateCompensation(request.HourlyRate, request.IsEligibleForCommission);

        _repository.Update(employeeProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
