using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.RedeemPoints;

public class RedeemPointsCommandHandler : ICommandHandler<RedeemPointsCommand>
{
    private readonly PosDbContext _context;

    public RedeemPointsCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RedeemPointsCommand request, CancellationToken cancellationToken)
    {
        var customerLoyalty = await _context.CustomerLoyalties
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (customerLoyalty == null)
        {
            throw new InvalidOperationException($"Customer is not enrolled in the loyalty program");
        }

        customerLoyalty.RedeemPoints(request.Points, request.Description, request.OrderId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
