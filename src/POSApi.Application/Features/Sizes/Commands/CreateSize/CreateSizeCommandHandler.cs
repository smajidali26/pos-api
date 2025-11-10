using POSApi.Domain.Entities;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Sizes.Commands.CreateSize;

public class CreateSizeCommandHandler : ICommandHandler<CreateSizeCommand, SizeDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSizeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SizeDto> Handle(CreateSizeCommand request, CancellationToken cancellationToken)
    {
        // Check if size name already exists
        var existingSize = await _unitOfWork.Sizes.GetByNameAsync(request.Name, cancellationToken);
        if (existingSize != null)
        {
            throw new InvalidOperationException($"Size with name '{request.Name}' already exists");
        }

        var size = new Size(request.Name, request.Description);

        await _unitOfWork.Sizes.AddAsync(size, cancellationToken);
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
