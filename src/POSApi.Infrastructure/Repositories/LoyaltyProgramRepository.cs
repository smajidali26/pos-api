using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class LoyaltyProgramRepository : Repository<LoyaltyProgram>, ILoyaltyProgramRepository
{
    public LoyaltyProgramRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<LoyaltyProgram?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(lp => lp.IsActive, cancellationToken);
    }
}
