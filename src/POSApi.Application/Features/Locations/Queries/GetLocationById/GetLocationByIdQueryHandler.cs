using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IQueryHandler<GetLocationByIdQuery, LocationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetLocationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<LocationDto> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations
            .Include(l => l.ParentLocation)
            .Include(l => l.ProductLocations)
                .ThenInclude(pl => pl.Product)
            .FirstOrDefaultAsync(l => l.Id == request.LocationId, cancellationToken);

        if (location == null)
            throw new InvalidOperationException($"Location with ID {request.LocationId} not found");

        return _mapper.Map<LocationDto>(location);
    }
}
