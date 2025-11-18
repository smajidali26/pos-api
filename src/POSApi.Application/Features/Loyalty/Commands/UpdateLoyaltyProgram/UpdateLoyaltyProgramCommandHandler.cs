using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.UpdateLoyaltyProgram;

public class UpdateLoyaltyProgramCommandHandler : ICommandHandler<UpdateLoyaltyProgramCommand>
{
    private readonly PosDbContext _context;

    public UpdateLoyaltyProgramCommandHandler(PosDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateLoyaltyProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await _context.LoyaltyPrograms
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (program == null)
        {
            throw new InvalidOperationException($"Loyalty program with ID {request.Id} not found");
        }

        program.UpdateDetails(request.Name, request.Description, request.MinimumPurchaseAmount);
        program.UpdateRules(request.PointsPerDollar, request.PointsExpiryDays);

        if (request.IsActive && !program.IsActive)
        {
            program.Activate();
        }
        else if (!request.IsActive && program.IsActive)
        {
            program.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
