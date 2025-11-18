using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.EmployeeProfiles.Queries;

public record GetAllEmployeeProfilesQuery : IRequest<List<EmployeeProfileDto>>
{
    public EmploymentStatus? Status { get; init; }
    public Guid? StoreId { get; init; }
    public Guid? ManagerId { get; init; }
}

public class GetAllEmployeeProfilesQueryHandler : IRequestHandler<GetAllEmployeeProfilesQuery, List<EmployeeProfileDto>>
{
    private readonly IEmployeeProfileRepository _repository;

    public GetAllEmployeeProfilesQueryHandler(IEmployeeProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EmployeeProfileDto>> Handle(GetAllEmployeeProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllWithUserAsync(cancellationToken);

        // Apply filters
        if (request.Status.HasValue)
            profiles = profiles.Where(p => p.Status == request.Status.Value).ToList();

        if (request.StoreId.HasValue)
            profiles = profiles.Where(p => p.StoreId == request.StoreId.Value).ToList();

        if (request.ManagerId.HasValue)
            profiles = profiles.Where(p => p.ManagerId == request.ManagerId.Value).ToList();

        return profiles.Select(p => new EmployeeProfileDto
        {
            Id = p.Id,
            UserId = p.UserId,
            Username = p.User?.Username ?? string.Empty,
            FullName = p.User?.FullName ?? string.Empty,
            Email = p.User?.Email ?? string.Empty,
            EmployeeCode = p.EmployeeCode,
            HireDate = p.HireDate,
            TerminationDate = p.TerminationDate,
            Status = p.Status.ToString(),
            EmploymentType = p.EmploymentType.ToString(),
            Department = p.Department,
            JobTitle = p.JobTitle,
            ManagerId = p.ManagerId,
            StoreId = p.StoreId,
            StoreName = p.Store?.Name,
            PhoneNumber = p.PhoneNumber,
            HourlyRate = p.HourlyRate,
            IsEligibleForCommission = p.IsEligibleForCommission,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}

public class EmployeeProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public bool IsEligibleForCommission { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
