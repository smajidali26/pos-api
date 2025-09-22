using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Commands.UpdateProductPricing;

public class UpdateProductPricingCommandHandler : ICommandHandler<UpdateProductPricingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductPricingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProductPricingCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        product.UpdatePrice(request.Price);
        product.UpdateCost(request.Cost);

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}