using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetLowStockProducts;

public class GetLowStockProductsQueryHandler : IQueryHandler<GetLowStockProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetLowStockProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);

        if (!request.IncludeOutOfStock)
        {
            products = products.Where(p => p.StockQuantity > 0);
        }

        return _mapper.Map<IEnumerable<ProductDto>>(products.OrderBy(p => p.StockQuantity));
    }
}