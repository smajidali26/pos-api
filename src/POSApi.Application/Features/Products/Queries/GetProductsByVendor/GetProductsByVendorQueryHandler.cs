using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetProductsByVendor;

public class GetProductsByVendorQueryHandler : IQueryHandler<GetProductsByVendorQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsByVendorQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsByVendorQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsByVendorAsync(request.VendorId, cancellationToken);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}
