using POSApi.Domain.Entities;

namespace POSApi.Domain.Repositories;

public interface IEmployeeProfileRepository : IRepository<EmployeeProfile>
{
    Task<EmployeeProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetAllWithUserAsync(CancellationToken cancellationToken = default);
    Task<EmployeeProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);
}
