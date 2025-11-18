using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.UpdateReward;

public class UpdateRewardCommandHandler : ICommandHandler<UpdateRewardCommand>
{
    private readonly PosDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRewardCommandHandler(PosDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (reward == null)
        {
            throw new InvalidOperationException($"Reward with ID {request.Id} not found");
        }

        // If product-based reward, verify product exists
        if (request.ProductId.HasValue)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
            }
        }

        reward.Update(
            request.Name,
            request.Description,
            request.PointsCost,
            request.RewardType,
            request.Value,
            request.ValidFrom,
            request.ValidTo,
            request.ProductId,
            request.MaxRedemptionsPerCustomer,
            request.TotalRedemptionsAllowed);

        if (request.IsActive && !reward.IsActive)
        {
            reward.Activate();
        }
        else if (!request.IsActive && reward.IsActive)
        {
            reward.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
