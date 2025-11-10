using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Sizes.Queries.GetSizeById;

public class GetSizeByIdQueryHandler : IQueryHandler<GetSizeByIdQuery, SizeDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSizeByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SizeDto?> Handle(GetSizeByIdQuery request, CancellationToken cancellationToken)
    {
        var size = await _unitOfWork.Sizes.GetByIdAsync(request.Id, cancellationToken);

        if (size == null)
        {
            return null;
        }

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
