using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Repositories;

public class EmployeeProfileRepository : Repository<EmployeeProfile>, IEmployeeProfileRepository
{
    public EmployeeProfileRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<EmployeeProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.User)
            .Include(e => e.Manager)
            .Include(e => e.Store)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<List<EmployeeProfile>> GetAllWithUserAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.User)
            .Include(e => e.Store)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.User)
            .Include(e => e.Manager)
            .Include(e => e.Store)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<EmployeeProfile>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.User)
            .Where(e => e.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmployeeProfile>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.User)
            .Where(e => e.ManagerId == managerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmployeeProfile>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.User)
            .Where(e => e.Status == EmploymentStatus.Active)
            .ToListAsync(cancellationToken);
    }
}
