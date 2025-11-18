using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Queries;

public record GetActiveCommissionsQuery : IRequest<List<CommissionDto>>
{
}

public class GetActiveCommissionsQueryHandler : IRequestHandler<GetActiveCommissionsQuery, List<CommissionDto>>
{
    private readonly ICommissionRepository _repository;

    public GetActiveCommissionsQueryHandler(ICommissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CommissionDto>> Handle(GetActiveCommissionsQuery request, CancellationToken cancellationToken)
    {
        var commissions = await _repository.GetActiveCommissionsAsync(cancellationToken);

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
