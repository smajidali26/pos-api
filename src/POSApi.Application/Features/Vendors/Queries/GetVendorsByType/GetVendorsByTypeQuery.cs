using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Vendors.Queries.GetVendorsByType;

public class GetVendorsByTypeQuery : IQuery<IEnumerable<VendorDto>>
{
    public VendorType Type { get; }

    public GetVendorsByTypeQuery(VendorType type)
    {
        Type = type;
    }
}