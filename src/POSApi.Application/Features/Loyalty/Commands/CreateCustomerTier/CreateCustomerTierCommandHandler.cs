using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.CreateCustomerTier;

public class CreateCustomerTierCommandHandler : ICommandHandler<CreateCustomerTierCommand, Guid>
{
    private readonly PosDbContext _context;

    public CreateCustomerTierCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCustomerTierCommand request, CancellationToken cancellationToken)
    {
        var tier = new CustomerTier(
            request.Name,
            request.MinSpend,
            request.MinPoints,
            request.BenefitMultiplier,
            request.DiscountPercentage,
            request.Color,
            request.SortOrder);

        _context.CustomerTiers.Add(tier);
        await _context.SaveChangesAsync(cancellationToken);

        return tier.Id;
    }
}
