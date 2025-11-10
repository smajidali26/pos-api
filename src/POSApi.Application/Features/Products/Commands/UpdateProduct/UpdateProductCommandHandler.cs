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

        // Validate size exists (if provided)
        if (request.SizeId.HasValue)
        {
            var size = await _unitOfWork.Sizes.GetByIdAsync(request.SizeId.Value, cancellationToken);
            if (size == null)
            {
                throw new InvalidOperationException($"Size with ID {request.SizeId} not found");
            }
        }

        // Update product using domain methods
        product.UpdateBasicInfo(request.Name, request.Description, request.SizeId);
        product.UpdatePrice(request.Price);
        product.UpdateCost(request.Cost);
        product.UpdateCategory(request.CategoryId);

        // Update vendor if provided
        if (request.PrimaryVendorId.HasValue)
        {
            product.SetPrimaryVendor(request.PrimaryVendorId.Value);
        }

        // Use reflection for MinStockLevel (in a real app, you'd add a domain method for this)
        var productType = product.GetType();
        productType.GetProperty("MinStockLevel")?.SetValue(product, request.MinStockLevel);

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}