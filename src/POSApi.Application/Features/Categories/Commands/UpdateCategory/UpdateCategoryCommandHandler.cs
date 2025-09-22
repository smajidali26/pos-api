using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {request.Id} not found");
        }

        // Check if parent category exists (if provided)
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                throw new InvalidOperationException($"Parent category with ID {request.ParentCategoryId.Value} not found");
            }

            // Prevent circular reference
            if (request.ParentCategoryId.Value == request.Id)
            {
                throw new InvalidOperationException("Category cannot be its own parent");
            }
        }

        // Check if name is unique (excluding current category)
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(request.Name, cancellationToken);
        if (existingCategory != null && existingCategory.Id != request.Id)
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
        }

        // Update category using domain methods
        category.UpdateName(request.Name);
        category.UpdateDescription(request.Description);
        
        // Update parent category (using reflection as there's no domain method for this)
        var categoryType = category.GetType();
        categoryType.GetProperty("ParentCategoryId")?.SetValue(category, request.ParentCategoryId);

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}