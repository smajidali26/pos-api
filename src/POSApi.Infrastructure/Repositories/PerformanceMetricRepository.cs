using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Repositories;

public class PerformanceMetricRepository : Repository<PerformanceMetric>, IPerformanceMetricRepository
{
    public PerformanceMetricRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<List<PerformanceMetric>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.EmployeeProfile).ThenInclude(e => e.User)
            .Where(m => m.EmployeeProfileId == employeeProfileId)
            .OrderByDescending(m => m.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<PerformanceMetric?> GetByPeriodAsync(Guid employeeProfileId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.EmployeeProfile).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(m => m.EmployeeProfileId == employeeProfileId &&
                                     m.PeriodStart == periodStart &&
                                     m.PeriodEnd == periodEnd,
                                cancellationToken);
    }

    public async Task<List<PerformanceMetric>> GetTopPerformersAsync(DateTime periodStart, DateTime periodEnd, int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.EmployeeProfile).ThenInclude(e => e.User)
            .Where(m => m.PeriodStart == periodStart && m.PeriodEnd == periodEnd)
            .OrderByDescending(m => m.PerformanceScore)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
