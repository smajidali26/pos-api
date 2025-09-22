using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Queries.GetAllVendors;

public class GetAllVendorsQueryHandler : IQueryHandler<GetAllVendorsQuery, IEnumerable<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllVendorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VendorDto>> Handle(GetAllVendorsQuery request, CancellationToken cancellationToken)
    {
        var vendors = request.IncludeInactive
            ? await _unitOfWork.Vendors.GetAllAsync(cancellationToken)
            : await _unitOfWork.Vendors.GetActiveVendorsAsync(cancellationToken);

        return _mapper.Map<IEnumerable<VendorDto>>(vendors);
    }
}