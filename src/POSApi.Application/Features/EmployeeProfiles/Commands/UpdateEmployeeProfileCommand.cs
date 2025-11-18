using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.EmployeeProfiles.Commands;

public record UpdateEmployeeProfileCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public Guid? ManagerId { get; init; }
    public Guid? StoreId { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
}

public class UpdateEmployeeProfileCommandHandler : IRequestHandler<UpdateEmployeeProfileCommand, Unit>
{
    private readonly IEmployeeProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeProfileCommandHandler(
        IEmployeeProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateEmployeeProfileCommand request, CancellationToken cancellationToken)
    {
        var employeeProfile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (employeeProfile == null)
            throw new KeyNotFoundException($"Employee profile with ID {request.Id} not found");

        employeeProfile.UpdateProfile(
            request.PhoneNumber,
            request.Department,
            request.JobTitle,
            request.ManagerId,
            request.StoreId,
            request.Address,
            request.City,
            request.State,
            request.ZipCode,
            request.Country
        );

        _repository.Update(employeeProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
