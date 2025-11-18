using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetUnitTypes;

public class GetUnitTypesQueryHandler : IQueryHandler<GetUnitTypesQuery, IEnumerable<UnitTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnitTypesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UnitTypeDto>> Handle(GetUnitTypesQuery request, CancellationToken cancellationToken)
    {
        var unitTypes = await _unitOfWork.UnitTypes.GetActiveAsync(cancellationToken);

        var result = unitTypes.Select(ut => new UnitTypeDto
        {
            Id = ut.Id,
            Name = ut.Name,
            Description = ut.Description,
            SortOrder = ut.SortOrder
        });

        return result;
    }
}
