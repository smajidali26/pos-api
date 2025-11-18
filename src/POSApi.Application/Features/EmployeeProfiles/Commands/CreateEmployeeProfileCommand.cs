using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.EmployeeProfiles.Commands;

public record CreateEmployeeProfileCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public DateTime HireDate { get; init; }
    public EmploymentStatus Status { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public decimal HourlyRate { get; init; }
    public bool IsEligibleForCommission { get; init; }
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public Guid? ManagerId { get; init; }
    public Guid? StoreId { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
}

public class CreateEmployeeProfileCommandHandler : IRequestHandler<CreateEmployeeProfileCommand, Guid>
{
    private readonly IEmployeeProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeProfileCommandHandler(
        IEmployeeProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateEmployeeProfileCommand request, CancellationToken cancellationToken)
    {
        // Check if employee profile already exists for this user
        var existing = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Employee profile already exists for user {request.UserId}");

        var employeeProfile = new EmployeeProfile(
            request.UserId,
            request.EmployeeCode,
            request.HireDate,
            request.Status,
            request.EmploymentType,
            request.PhoneNumber,
            request.HourlyRate,
            request.IsEligibleForCommission,
            request.Department,
            request.JobTitle,
            request.ManagerId,
            request.StoreId
        );

        // Update additional contact information if provided
        if (!string.IsNullOrWhiteSpace(request.Address))
        {
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
        }

        if (!string.IsNullOrWhiteSpace(request.EmergencyContactName))
        {
            employeeProfile.UpdateEmergencyContact(
                request.EmergencyContactName,
                request.EmergencyContactPhone ?? string.Empty
            );
        }

        await _repository.AddAsync(employeeProfile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employeeProfile.Id;
    }
}
