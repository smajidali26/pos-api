using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Queries.GetLocationProducts;

public class GetLocationProductsQueryHandler : IQueryHandler<GetLocationProductsQuery, IEnumerable<ProductLocationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetLocationProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductLocationDto>> Handle(GetLocationProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.ProductLocations
            .Include(pl => pl.Product)
            .Include(pl => pl.Location)
            .Where(pl => pl.LocationId == request.LocationId);

        if (request.IsLowStock.HasValue)
            query = query.Where(pl => pl.IsLowStock == request.IsLowStock.Value);

        if (request.IsOutOfStock.HasValue)
            query = query.Where(pl => pl.IsOutOfStock == request.IsOutOfStock.Value);

        if (request.IsOverStock.HasValue)
            query = query.Where(pl => pl.IsOverStock == request.IsOverStock.Value);

        var productLocations = await query
            .OrderBy(pl => pl.Product.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductLocationDto>>(productLocations);
    }
}
