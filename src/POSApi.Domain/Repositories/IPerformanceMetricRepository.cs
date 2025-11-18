using POSApi.Domain.Entities;

namespace POSApi.Domain.Repositories;

public interface IPerformanceMetricRepository : IRepository<PerformanceMetric>
{
    Task<List<PerformanceMetric>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<PerformanceMetric?> GetByPeriodAsync(Guid employeeProfileId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
    Task<List<PerformanceMetric>> GetTopPerformersAsync(DateTime periodStart, DateTime periodEnd, int count, CancellationToken cancellationToken = default);
}
