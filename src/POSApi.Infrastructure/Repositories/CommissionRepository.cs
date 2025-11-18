using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Repositories;

public class CommissionRepository : Repository<Commission>, ICommissionRepository
{
    public CommissionRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<List<Commission>> GetActiveCommissionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(c => c.EmployeeProfile)
            .Include(c => c.Product)
            .Include(c => c.Category)
            .Where(c => c.IsActive &&
                       c.EffectiveFrom <= now &&
                       (!c.EffectiveTo.HasValue || c.EffectiveTo.Value >= now))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Commission>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Product)
            .Include(c => c.Category)
            .Where(c => c.EmployeeProfileId == employeeProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Commission>> GetApplicableCommissionsAsync(Guid? employeeProfileId, string? role, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = _dbSet
            .Include(c => c.Product)
            .Include(c => c.Category)
            .Where(c => c.IsActive &&
                       c.EffectiveFrom <= now &&
                       (!c.EffectiveTo.HasValue || c.EffectiveTo.Value >= now));

        if (employeeProfileId.HasValue)
        {
            query = query.Where(c => !c.EmployeeProfileId.HasValue || c.EmployeeProfileId == employeeProfileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(c => string.IsNullOrEmpty(c.Role) || c.Role == role);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
