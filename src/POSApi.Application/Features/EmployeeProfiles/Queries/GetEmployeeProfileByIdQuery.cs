using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.EmployeeProfiles.Queries;

public record GetEmployeeProfileByIdQuery(Guid Id) : IRequest<EmployeeProfileDetailDto?>;

public class GetEmployeeProfileByIdQueryHandler : IRequestHandler<GetEmployeeProfileByIdQuery, EmployeeProfileDetailDto?>
{
    private readonly IEmployeeProfileRepository _repository;

    public GetEmployeeProfileByIdQueryHandler(IEmployeeProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmployeeProfileDetailDto?> Handle(GetEmployeeProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (profile == null)
            return null;

        return new EmployeeProfileDetailDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Username = profile.User?.Username ?? string.Empty,
            FullName = profile.User?.FullName ?? string.Empty,
            Email = profile.User?.Email ?? string.Empty,
            Role = profile.User?.Role.ToString(),
            EmployeeCode = profile.EmployeeCode,
            HireDate = profile.HireDate,
            TerminationDate = profile.TerminationDate,
            Status = profile.Status.ToString(),
            EmploymentType = profile.EmploymentType.ToString(),
            Department = profile.Department,
            JobTitle = profile.JobTitle,
            ManagerId = profile.ManagerId,
            ManagerName = profile.Manager?.FullName,
            StoreId = profile.StoreId,
            StoreName = profile.Store?.Name,
            PhoneNumber = profile.PhoneNumber,
            EmergencyContactName = profile.EmergencyContactName,
            EmergencyContactPhone = profile.EmergencyContactPhone,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country,
            HourlyRate = profile.HourlyRate,
            IsEligibleForCommission = profile.IsEligibleForCommission,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}

public class EmployeeProfileDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsEligibleForCommission { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
