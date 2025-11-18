using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdQuery : IQuery<LocationDto>
{
    public Guid LocationId { get; set; }
}
