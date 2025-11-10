using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Sizes.Commands.UpdateSize;

public class UpdateSizeCommandHandler : ICommandHandler<UpdateSizeCommand, SizeDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSizeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SizeDto> Handle(UpdateSizeCommand request, CancellationToken cancellationToken)
    {
        var size = await _unitOfWork.Sizes.GetByIdAsync(request.Id, cancellationToken);
        if (size == null)
        {
            throw new InvalidOperationException($"Size with ID {request.Id} not found");
        }

        // Check if new name conflicts with another size
        var existingSize = await _unitOfWork.Sizes.GetByNameAsync(request.Name, cancellationToken);
        if (existingSize != null && existingSize.Id != request.Id)
        {
            throw new InvalidOperationException($"Size with name '{request.Name}' already exists");
        }

        size.UpdateName(request.Name);
        size.UpdateDescription(request.Description);

        await _unitOfWork.Sizes.UpdateAsync(size, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SizeDto
        {
            Id = size.Id,
            Name = size.Name,
            Description = size.Description,
            IsActive = size.IsActive,
            CreatedAt = size.CreatedAt,
            UpdatedAt = size.UpdatedAt
        };
    }
}
