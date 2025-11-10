using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Sizes.Queries.GetAllSizes;

public class GetAllSizesQueryHandler : IQueryHandler<GetAllSizesQuery, IEnumerable<SizeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSizesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SizeDto>> Handle(GetAllSizesQuery request, CancellationToken cancellationToken)
    {
        var sizes = await _unitOfWork.Sizes.GetAllAsync(cancellationToken);

        return sizes.Select(s => new SizeDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}
