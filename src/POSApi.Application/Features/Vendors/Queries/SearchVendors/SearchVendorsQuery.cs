using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Vendors.Queries.SearchVendors;

public class SearchVendorsQuery : IQuery<IEnumerable<VendorDto>>
{
    public string SearchTerm { get; }

    public SearchVendorsQuery(string searchTerm)
    {
        SearchTerm = searchTerm ?? string.Empty;
    }
}