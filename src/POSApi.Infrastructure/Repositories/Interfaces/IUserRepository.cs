using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}