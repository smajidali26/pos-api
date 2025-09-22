using POSApi.Infrastructure.Persistence;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.UpdateStock;

public class UpdateStockCommandHandler : ICommandHandler<UpdateStockCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        product.UpdateStock(request.Quantity);

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}