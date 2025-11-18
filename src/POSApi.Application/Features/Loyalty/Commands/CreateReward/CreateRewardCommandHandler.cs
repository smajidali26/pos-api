using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.CreateReward;

public class CreateRewardCommandHandler : ICommandHandler<CreateRewardCommand, Guid>
{
    private readonly PosDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRewardCommandHandler(PosDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateRewardCommand request, CancellationToken cancellationToken)
    {
        // If product-based reward, verify product exists
        if (request.ProductId.HasValue)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
            }
        }

        var reward = new Reward(
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

        _context.Rewards.Add(reward);
        await _context.SaveChangesAsync(cancellationToken);

        return reward.Id;
    }
}
