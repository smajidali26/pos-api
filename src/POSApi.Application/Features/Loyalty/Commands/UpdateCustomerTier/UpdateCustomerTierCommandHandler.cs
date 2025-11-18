using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.UpdateCustomerTier;

public class UpdateCustomerTierCommandHandler : ICommandHandler<UpdateCustomerTierCommand>
{
    private readonly PosDbContext _context;

    public UpdateCustomerTierCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCustomerTierCommand request, CancellationToken cancellationToken)
    {
        var tier = await _context.CustomerTiers
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tier == null)
        {
            throw new InvalidOperationException($"Customer tier with ID {request.Id} not found");
        }

        tier.Update(
            request.Name,
            request.MinSpend,
            request.MinPoints,
            request.BenefitMultiplier,
            request.DiscountPercentage,
            request.Color,
            request.SortOrder);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
