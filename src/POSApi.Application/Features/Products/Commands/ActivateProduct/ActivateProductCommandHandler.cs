using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Commands.ActivateProduct;

public class ActivateProductCommandHandler : ICommandHandler<ActivateProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        product.Activate();

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}