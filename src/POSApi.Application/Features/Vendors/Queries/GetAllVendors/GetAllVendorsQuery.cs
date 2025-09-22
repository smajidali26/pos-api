using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Vendors.Queries.GetAllVendors;

public class GetAllVendorsQuery : IQuery<IEnumerable<VendorDto>>
{
    public bool IncludeInactive { get; set; } = false;
}