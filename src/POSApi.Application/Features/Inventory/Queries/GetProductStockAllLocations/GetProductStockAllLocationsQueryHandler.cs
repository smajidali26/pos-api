using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Inventory.Queries.GetProductStockAllLocations;

public class GetProductStockAllLocationsQueryHandler : IQueryHandler<GetProductStockAllLocationsQuery, IEnumerable<ProductLocationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductStockAllLocationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductLocationDto>> Handle(GetProductStockAllLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.ProductLocations
            .Include(pl => pl.Product)
            .Include(pl => pl.Location)
            .Where(pl => pl.ProductId == request.ProductId);

        if (!request.IncludeInactive)
            query = query.Where(pl => pl.Location.IsActive);

        var productLocations = await query
            .OrderBy(pl => pl.Location.Name)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductLocationDto>>(productLocations);
    }
}
