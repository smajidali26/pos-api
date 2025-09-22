using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetProductByBarcode;

public class GetProductByBarcodeQueryHandler : IQueryHandler<GetProductByBarcodeQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByBarcodeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByBarcodeAsync(request.Barcode, cancellationToken);
        
        if (product == null)
            return null;

        return _mapper.Map<ProductDto>(product);
    }
}