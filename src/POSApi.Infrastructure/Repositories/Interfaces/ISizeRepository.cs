using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface ISizeRepository : IRepository<Size>
{
    Task<Size?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
