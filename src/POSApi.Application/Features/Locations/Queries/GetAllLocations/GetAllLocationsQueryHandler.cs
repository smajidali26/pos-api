using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Queries.GetAllLocations;

public class GetAllLocationsQueryHandler : IQueryHandler<GetAllLocationsQuery, IEnumerable<LocationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllLocationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LocationDto>> Handle(GetAllLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.Locations.AsQueryable();

        if (request.LocationType.HasValue)
            query = query.Where(l => l.LocationType == request.LocationType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(l => l.IsActive == request.IsActive.Value);

        if (request.ParentLocationId.HasValue)
            query = query.Where(l => l.ParentLocationId == request.ParentLocationId.Value);

        var locations = await query
            .Include(l => l.ParentLocation)
            .Include(l => l.ProductLocations)
            .OrderBy(l => l.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }
}
