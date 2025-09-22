using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Queries.GetVendorsByType;

public class GetVendorsByTypeQueryHandler : IQueryHandler<GetVendorsByTypeQuery, IEnumerable<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetVendorsByTypeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VendorDto>> Handle(GetVendorsByTypeQuery request, CancellationToken cancellationToken)
    {
        var vendors = await _unitOfWork.Vendors.GetByTypeAsync(request.Type, cancellationToken);
        return _mapper.Map<IEnumerable<VendorDto>>(vendors);
    }
}