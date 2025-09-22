using POSApi.Infrastructure.Persistence;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.Id} not found");
        }

        // Check if category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");
        }

        // Update product using domain methods
        product.UpdatePrice(request.Price);
        
        // Use reflection to update other properties (in a real app, you'd add domain methods for these)
        var productType = product.GetType();
        productType.GetProperty("Name")?.SetValue(product, request.Name);
        productType.GetProperty("Description")?.SetValue(product, request.Description);
        productType.GetProperty("Cost")?.SetValue(product, request.Cost);
        productType.GetProperty("MinStockLevel")?.SetValue(product, request.MinStockLevel);
        productType.GetProperty("CategoryId")?.SetValue(product, request.CategoryId);

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}