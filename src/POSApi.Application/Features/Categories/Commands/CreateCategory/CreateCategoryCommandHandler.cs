using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if category name already exists
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(request.Name, cancellationToken);
        if (existingCategory != null)
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
        }

        // Check if parent category exists if provided
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                throw new InvalidOperationException($"Parent category with ID {request.ParentCategoryId} not found");
            }
        }

        var category = new Category(request.Name, request.Description, request.ParentCategoryId);

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}