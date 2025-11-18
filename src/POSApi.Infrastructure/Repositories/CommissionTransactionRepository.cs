using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Repositories;

public class CommissionTransactionRepository : Repository<CommissionTransaction>, ICommissionTransactionRepository
{
    public CommissionTransactionRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<List<CommissionTransaction>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Commission)
            .Include(t => t.Order)
            .Where(t => t.EmployeeProfileId == employeeProfileId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CommissionTransaction>> GetPendingCommissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.EmployeeProfile).ThenInclude(e => e.User)
            .Include(t => t.Commission)
            .Include(t => t.Order)
            .Where(t => t.Status == CommissionStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CommissionTransaction>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Commission)
            .Where(t => t.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalEarnedAsync(Guid employeeProfileId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.EmployeeProfileId == employeeProfileId &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate < endDate &&
                       (t.Status == CommissionStatus.Approved || t.Status == CommissionStatus.Paid))
            .SumAsync(t => t.CommissionAmount, cancellationToken);
    }

    public async Task<List<CommissionTransaction>> GetByEmployeeAndDateAsync(Guid employeeProfileId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _dbSet
            .Where(t => t.EmployeeProfileId == employeeProfileId &&
                       t.TransactionDate >= startOfDay &&
                       t.TransactionDate < endOfDay)
            .ToListAsync(cancellationToken);
    }
}
