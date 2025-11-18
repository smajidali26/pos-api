using POSApi.Domain.Entities;

namespace POSApi.Domain.Repositories;

public interface ICommissionRepository : IRepository<Commission>
{
    Task<List<Commission>> GetActiveCommissionsAsync(CancellationToken cancellationToken = default);
    Task<List<Commission>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<List<Commission>> GetApplicableCommissionsAsync(Guid? employeeProfileId, string? role, CancellationToken cancellationToken = default);
}
