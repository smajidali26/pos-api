using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetProductsNeedingReorder;

public class GetProductsNeedingReorderQueryHandler : IQueryHandler<GetProductsNeedingReorderQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsNeedingReorderQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsNeedingReorderQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsNeedingReorderAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}
