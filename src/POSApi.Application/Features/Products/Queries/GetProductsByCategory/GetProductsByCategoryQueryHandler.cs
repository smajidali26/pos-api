using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetProductsByCategory;

public class GetProductsByCategoryQueryHandler : IQueryHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsByCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetByCategoryIdAsync(request.CategoryId, cancellationToken);

        if (!request.IncludeInactive)
        {
            products = products.Where(p => p.IsActive);
        }

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}