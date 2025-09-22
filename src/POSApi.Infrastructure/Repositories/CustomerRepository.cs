using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.FirstName.Contains(searchTerm) || 
                       c.LastName.Contains(searchTerm) || 
                       c.Email.Contains(searchTerm) ||
                       c.PhoneNumber.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetTopLoyaltyCustomersAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(c => c.LoyaltyPoints)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}