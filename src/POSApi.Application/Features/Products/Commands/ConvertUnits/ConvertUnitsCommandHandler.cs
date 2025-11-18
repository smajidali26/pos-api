using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Commands.ConvertUnits;

public class ConvertUnitsCommandHandler : ICommandHandler<ConvertUnitsCommand, UnitConversionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConvertUnitsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitConversionResultDto> Handle(ConvertUnitsCommand request, CancellationToken cancellationToken)
    {
        var fromUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.FromUnitCode, cancellationToken);
        var toUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.ToUnitCode, cancellationToken);

        if (fromUnit == null)
        {
            throw new InvalidOperationException($"Invalid from unit code: {request.FromUnitCode}");
        }

        if (toUnit == null)
        {
            throw new InvalidOperationException($"Invalid to unit code: {request.ToUnitCode}");
        }

        var convertedQuantity = fromUnit.ConvertTo(request.Quantity, toUnit);

        var result = new UnitConversionResultDto
        {
            FromQuantity = request.Quantity,
            FromUnit = fromUnit.Symbol,
            ToQuantity = convertedQuantity,
            ToUnit = toUnit.Symbol,
            ConversionFactor = convertedQuantity / request.Quantity
        };

        return result;
    }
}
