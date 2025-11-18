using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Queries.GetProductsByVendor;

public class GetProductsByVendorQuery : IQuery<IEnumerable<ProductDto>>
{
    public Guid VendorId { get; }

    public GetProductsByVendorQuery(Guid vendorId)
    {
        VendorId = vendorId;
    }
}
