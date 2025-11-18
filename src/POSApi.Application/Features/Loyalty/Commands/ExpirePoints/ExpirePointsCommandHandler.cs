using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.ExpirePoints;

public class ExpirePointsCommandHandler : ICommandHandler<ExpirePointsCommand, int>
{
    private readonly PosDbContext _context;

    public ExpirePointsCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(ExpirePointsCommand request, CancellationToken cancellationToken)
    {
        var customerLoyalties = await _context.CustomerLoyalties
            .Include(cl => cl.Transactions)
            .ToListAsync(cancellationToken);

        int totalCustomersProcessed = 0;
        int totalPointsExpired = 0;

        foreach (var customerLoyalty in customerLoyalties)
        {
            var expiredPoints = customerLoyalty.ExpirePoints();
            if (expiredPoints > 0)
            {
                totalCustomersProcessed++;
                totalPointsExpired += expiredPoints;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return totalPointsExpired;
    }
}
