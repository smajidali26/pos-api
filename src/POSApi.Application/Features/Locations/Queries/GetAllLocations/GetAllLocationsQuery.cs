using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Locations.Queries.GetAllLocations;

public class GetAllLocationsQuery : IQuery<IEnumerable<LocationDto>>
{
    public LocationType? LocationType { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentLocationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
