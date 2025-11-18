using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Queries;

public record GetAllCommissionsQuery : IRequest<List<CommissionDto>>
{
    public bool? IsActive { get; init; }
    public Guid? EmployeeProfileId { get; init; }
}

public class GetAllCommissionsQueryHandler : IRequestHandler<GetAllCommissionsQuery, List<CommissionDto>>
{
    private readonly ICommissionRepository _repository;

    public GetAllCommissionsQueryHandler(ICommissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CommissionDto>> Handle(GetAllCommissionsQuery request, CancellationToken cancellationToken)
    {
        var commissions = await _repository.GetAllAsync(cancellationToken);

        if (request.IsActive.HasValue)
            commissions = commissions.Where(c => c.IsActive == request.IsActive.Value).ToList();

        if (request.EmployeeProfileId.HasValue)
            commissions = commissions.Where(c => c.EmployeeProfileId == request.EmployeeProfileId.Value || c.EmployeeProfileId == null).ToList();

        return commissions.Select(c => new CommissionDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CommissionType = c.CommissionType.ToString(),
            CommissionBasis = c.CommissionBasis.ToString(),
            Rate = c.Rate,
            MinimumSaleAmount = c.MinimumSaleAmount,
            MaximumCommission = c.MaximumCommission,
            IsActive = c.IsActive,
            EffectiveFrom = c.EffectiveFrom,
            EffectiveTo = c.EffectiveTo,
            EmployeeProfileId = c.EmployeeProfileId,
            EmployeeName = c.EmployeeProfile?.User?.FullName,
            ProductId = c.ProductId,
            ProductName = c.Product?.Name,
            CategoryId = c.CategoryId,
            CategoryName = c.Category?.Name,
            Role = c.Role,
            CreatedAt = c.CreatedAt
        }).OrderBy(c => c.Name).ToList();
    }
}

public class CommissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CommissionType { get; set; } = string.Empty;
    public string CommissionBasis { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal? MinimumSaleAmount { get; set; }
    public decimal? MaximumCommission { get; set; }
    public bool IsActive { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public Guid? EmployeeProfileId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
