using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Queries.GetVendorsWithCreditLimitExceeded;

public class GetVendorsWithCreditLimitExceededQueryHandler : IQueryHandler<GetVendorsWithCreditLimitExceededQuery, IEnumerable<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetVendorsWithCreditLimitExceededQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VendorDto>> Handle(GetVendorsWithCreditLimitExceededQuery request, CancellationToken cancellationToken)
    {
        var vendors = await _unitOfWork.Vendors.GetVendorsWithCreditLimitExceededAsync(cancellationToken);
        return _mapper.Map<IEnumerable<VendorDto>>(vendors);
    }
}
