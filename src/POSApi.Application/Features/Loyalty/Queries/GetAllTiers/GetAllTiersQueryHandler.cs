using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetAllTiers;

public class GetAllTiersQueryHandler : IQueryHandler<GetAllTiersQuery, List<CustomerTierDto>>
{
    private readonly PosDbContext _context;

    public GetAllTiersQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerTierDto>> Handle(GetAllTiersQuery request, CancellationToken cancellationToken)
    {
        var tiers = await _context.CustomerTiers
            .OrderBy(t => t.SortOrder)
            .Select(t => new CustomerTierDto
            {
                Id = t.Id,
                Name = t.Name,
                MinSpend = t.MinSpend,
                MinPoints = t.MinPoints,
                BenefitMultiplier = t.BenefitMultiplier,
                DiscountPercentage = t.DiscountPercentage,
                Color = t.Color,
                SortOrder = t.SortOrder,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return tiers;
    }
}
