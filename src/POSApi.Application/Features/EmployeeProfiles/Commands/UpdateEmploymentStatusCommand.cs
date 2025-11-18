using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.EmployeeProfiles.Commands;

public record UpdateEmploymentStatusCommand : IRequest<Unit>
{
    public Guid EmployeeProfileId { get; init; }
    public EmploymentStatus Status { get; init; }
    public DateTime? TerminationDate { get; init; }
}

public class UpdateEmploymentStatusCommandHandler : IRequestHandler<UpdateEmploymentStatusCommand, Unit>
{
    private readonly IEmployeeProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmploymentStatusCommandHandler(
        IEmployeeProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateEmploymentStatusCommand request, CancellationToken cancellationToken)
    {
        var employeeProfile = await _repository.GetByIdAsync(request.EmployeeProfileId, cancellationToken);
        if (employeeProfile == null)
            throw new KeyNotFoundException($"Employee profile with ID {request.EmployeeProfileId} not found");

        switch (request.Status)
        {
            case EmploymentStatus.Active:
                employeeProfile.Activate();
                break;
            case EmploymentStatus.Suspended:
                employeeProfile.Suspend();
                break;
            case EmploymentStatus.OnLeave:
                employeeProfile.PlaceOnLeave();
                break;
            case EmploymentStatus.Terminated:
                if (!request.TerminationDate.HasValue)
                    throw new ArgumentException("Termination date is required when terminating an employee");
                employeeProfile.Terminate(request.TerminationDate.Value);
                break;
        }

        _repository.Update(employeeProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
