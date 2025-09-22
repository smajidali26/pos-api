using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Queries.SearchVendors;

public class SearchVendorsQueryHandler : IQueryHandler<SearchVendorsQuery, IEnumerable<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchVendorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VendorDto>> Handle(SearchVendorsQuery request, CancellationToken cancellationToken)
    {
        var vendors = string.IsNullOrWhiteSpace(request.SearchTerm)
            ? await _unitOfWork.Vendors.GetActiveVendorsAsync(cancellationToken)
            : await _unitOfWork.Vendors.SearchVendorsAsync(request.SearchTerm, cancellationToken);

        return _mapper.Map<IEnumerable<VendorDto>>(vendors);
    }
}