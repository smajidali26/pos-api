using POSApi.Domain.Entities;

namespace POSApi.Domain.Repositories;

public interface ICommissionTransactionRepository : IRepository<CommissionTransaction>
{
    Task<List<CommissionTransaction>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<List<CommissionTransaction>> GetPendingCommissionsAsync(CancellationToken cancellationToken = default);
    Task<List<CommissionTransaction>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalEarnedAsync(Guid employeeProfileId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<CommissionTransaction>> GetByEmployeeAndDateAsync(Guid employeeProfileId, DateTime date, CancellationToken cancellationToken = default);
}
