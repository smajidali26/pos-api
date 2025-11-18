using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Queries.GetProductAnalytics;

public class GetProductAnalyticsQueryHandler : IQueryHandler<GetProductAnalyticsQuery, ProductAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductAnalyticsDto> Handle(GetProductAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var allProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var lowStockProducts = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);
        var productsNeedingReorder = await _unitOfWork.Products.GetProductsNeedingReorderAsync(cancellationToken);

        var allProductsList = allProducts.ToList();
        var activeProducts = allProductsList.Where(p => p.IsActive).ToList();

        var analytics = new ProductAnalyticsDto
        {
            TotalProducts = allProductsList.Count,
            ActiveProducts = activeProducts.Count,
            InactiveProducts = allProductsList.Count(p => !p.IsActive),
            LowStockProducts = lowStockProducts.Count(),
            OutOfStockProducts = allProductsList.Count(p => p.StockQuantity <= 0),
            ProductsNeedingReorder = productsNeedingReorder.Count(),
            TotalInventoryValue = activeProducts.Sum(p => p.GetInventoryValue()),
            AverageProductPrice = activeProducts.Where(p => p.Price > 0).Any()
                ? activeProducts.Where(p => p.Price > 0).Average(p => p.Price)
                : 0,
            TopCategories = allProductsList
                .Where(p => p.Category != null)
                .GroupBy(p => p.Category!.Name)
                .Select(g => new CategoryProductCount
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count()
                })
                .OrderByDescending(x => x.ProductCount)
                .Take(10)
                .ToList()
        };

        return analytics;
    }
}
