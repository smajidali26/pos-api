using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {request.Id} not found");
        }

        // Check if category has subcategories
        if (category.SubCategories.Any())
        {
            throw new InvalidOperationException("Cannot delete category that has subcategories. Please delete or reassign subcategories first.");
        }

        // Check if category has products assigned
        if (category.Products.Any())
        {
            throw new InvalidOperationException("Cannot delete category that has products assigned. Please reassign products to another category first.");
        }

        // Soft delete by deactivating the category
        category.Deactivate();
        
        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}