using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetProductBySku;

public class GetProductBySkuQueryHandler : IQueryHandler<GetProductBySkuQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductBySkuQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetBySkuAsync(request.Sku, cancellationToken);
        
        if (product == null)
            return null;

        return _mapper.Map<ProductDto>(product);
    }
}