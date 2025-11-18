using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface ILoyaltyProgramRepository : IRepository<LoyaltyProgram>
{
    Task<LoyaltyProgram?> GetActiveAsync(CancellationToken cancellationToken = default);
}
