using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Vendors.Queries.GetVendorById;

public class GetVendorByIdQuery : IQuery<VendorDto?>
{
    public Guid VendorId { get; }

    public GetVendorByIdQuery(Guid vendorId)
    {
        VendorId = vendorId;
    }
}