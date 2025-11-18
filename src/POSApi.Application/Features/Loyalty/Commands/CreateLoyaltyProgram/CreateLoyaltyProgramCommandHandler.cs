using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.CreateLoyaltyProgram;

public class CreateLoyaltyProgramCommandHandler : ICommandHandler<CreateLoyaltyProgramCommand, Guid>
{
    private readonly PosDbContext _context;

    public CreateLoyaltyProgramCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLoyaltyProgramCommand request, CancellationToken cancellationToken)
    {
        var program = new LoyaltyProgram(
            request.Name,
            request.Description,
            request.PointsPerDollar,
            request.MinimumPurchaseAmount,
            request.PointsExpiryDays);

        _context.LoyaltyPrograms.Add(program);
        await _context.SaveChangesAsync(cancellationToken);

        return program.Id;
    }
}
