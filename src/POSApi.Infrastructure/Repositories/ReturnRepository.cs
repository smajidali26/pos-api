using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class ReturnRepository : Repository<Return>, IReturnRepository
{
    public ReturnRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Return?> GetByReturnNumberAsync(string returnNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.ReturnNumber == returnNumber, cancellationToken);
    }

    public async Task<IEnumerable<Return>> GetByOriginalOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Include(r => r.ReturnItems)
            .Where(r => r.OriginalOrderId == orderId)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Return>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Include(r => r.ReturnItems)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Where(r => r.Status == status)
            .OrderBy(r => r.ReturnDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Return>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Where(r => r.ReturnDate >= startDate && r.ReturnDate <= endDate)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Return>> GetByProcessedUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Where(r => r.ProcessedByUserId == userId)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRefundsForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ReturnDate >= startDate && 
                       r.ReturnDate <= endDate &&
                       r.Status == ReturnStatus.Completed)
            .SumAsync(r => r.RefundAmount, cancellationToken);
    }

    public override async Task<Return?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Return>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.OriginalOrder)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedBy)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(cancellationToken);
    }
}