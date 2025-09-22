using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetTopLoyaltyCustomersAsync(int count, CancellationToken cancellationToken = default);
}