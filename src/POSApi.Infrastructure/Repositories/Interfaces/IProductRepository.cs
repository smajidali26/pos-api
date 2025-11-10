using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetBySizeIdAsync(Guid sizeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsNeedingReorderAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default);
    
    // New methods for pagination and advanced search
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(
        int skip, 
        int take, 
        string? searchTerm = null, 
        Guid? categoryId = null, 
        bool? isActive = null, 
        bool? isLowStock = null, 
        CancellationToken cancellationToken = default);
}