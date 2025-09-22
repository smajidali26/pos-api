using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default);
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}