using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.SearchProducts;

public class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Product> products;

        if (string.IsNullOrWhiteSpace(request.SearchTerm) && !request.CategoryId.HasValue && !request.IsLowStock.HasValue)
        {
            // Get all products if no search criteria provided
            products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        }
        else
        {
            // Start with search by term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                products = await _unitOfWork.Products.SearchProductsAsync(request.SearchTerm, cancellationToken);
            }
            else
            {
                products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            }

            // Apply additional filters
            if (request.CategoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.IsLowStock.HasValue && request.IsLowStock.Value)
            {
                products = products.Where(p => p.IsLowStock);
            }
        }

        // Apply active filter if specified
        if (request.IsActive.HasValue)
        {
            products = products.Where(p => p.IsActive == request.IsActive.Value);
        }

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}