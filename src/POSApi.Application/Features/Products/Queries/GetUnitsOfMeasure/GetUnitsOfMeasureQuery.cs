using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.DTOs;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetUnitsOfMeasure;

public class GetUnitsOfMeasureQuery : IQuery<IEnumerable<UnitOfMeasureDto>>
{
    public string? UnitTypeName { get; set; }
}

public class GetUnitsOfMeasureQueryHandler : IQueryHandler<GetUnitsOfMeasureQuery, IEnumerable<UnitOfMeasureDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnitsOfMeasureQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UnitOfMeasureDto>> Handle(GetUnitsOfMeasureQuery request, CancellationToken cancellationToken)
    {
        var units = string.IsNullOrEmpty(request.UnitTypeName) 
            ? await _unitOfWork.UnitsOfMeasure.GetActiveAsync(cancellationToken)
            : await _unitOfWork.UnitsOfMeasure.GetByUnitTypeNameAsync(request.UnitTypeName, cancellationToken);

        var result = units.Select(u => new UnitOfMeasureDto
        {
            Code = u.Code,
            Name = u.Name,
            Symbol = u.Symbol,
            Type = u.UnitType?.Name ?? string.Empty,
            ConversionFactorToBase = u.ConversionFactorToBase,
            IsBaseUnit = u.IsBaseUnit
        });

        return result;
    }
}