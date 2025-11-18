using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.AdjustPoints;

public class AdjustPointsCommandHandler : ICommandHandler<AdjustPointsCommand>
{
    private readonly PosDbContext _context;

    public AdjustPointsCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AdjustPointsCommand request, CancellationToken cancellationToken)
    {
        var customerLoyalty = await _context.CustomerLoyalties
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            throw new InvalidOperationException($"Customer is not enrolled in the loyalty program");
        }

        customerLoyalty.AdjustPoints(request.PointsAdjustment, request.Reason);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
