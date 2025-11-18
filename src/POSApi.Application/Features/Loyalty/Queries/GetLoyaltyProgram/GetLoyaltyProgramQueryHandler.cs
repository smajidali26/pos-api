using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Queries.GetLoyaltyProgram;

public class GetLoyaltyProgramQueryHandler : IQueryHandler<GetLoyaltyProgramQuery, LoyaltyProgramDto?>
{
    private readonly PosDbContext _context;

    public GetLoyaltyProgramQueryHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<LoyaltyProgramDto?> Handle(GetLoyaltyProgramQuery request, CancellationToken cancellationToken)
    {
        var program = await _context.LoyaltyPrograms
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (program == null)
        {
            return null;
        }

        return new LoyaltyProgramDto
        {
            Id = program.Id,
            Name = program.Name,
            Description = program.Description,
            PointsPerDollar = program.PointsPerDollar,
            MinimumPurchaseAmount = program.MinimumPurchaseAmount,
            PointsExpiryDays = program.PointsExpiryDays,
            IsActive = program.IsActive,
            CreatedAt = program.CreatedAt,
            UpdatedAt = program.UpdatedAt
        };
    }
}
