using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .FirstOrDefaultAsync(p => p.Barcode == barcode, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetBySizeIdAsync(Guid sizeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .Where(p => p.SizeId == sizeId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .Where(p => p.StockQuantity <= p.MinStockLevel && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerSearchTerm) || 
                p.SKU.ToLower().Contains(lowerSearchTerm) || 
                (p.Barcode != null && p.Barcode.ToLower().Contains(lowerSearchTerm)) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(lowerSearchTerm)) ||
                p.Description.ToLower().Contains(lowerSearchTerm));
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsNeedingReorderAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .Where(p => p.StockQuantity <= p.ReorderLevel && p.ReorderQuantity > 0 && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .Where(p => p.PrimaryVendorId == vendorId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(
        int skip,
        int take,
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isActive = null,
        bool? isLowStock = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .AsQueryable();

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerSearchTerm) || 
                p.SKU.ToLower().Contains(lowerSearchTerm) || 
                (p.Barcode != null && p.Barcode.ToLower().Contains(lowerSearchTerm)) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(lowerSearchTerm)) ||
                p.Description.ToLower().Contains(lowerSearchTerm));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Apply active status filter
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Apply low stock filter
        if (isLowStock.HasValue && isLowStock.Value)
        {
            query = query.Where(p => p.StockQuantity <= p.MinStockLevel);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var products = await query
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit!)
                    .ThenInclude(bu => bu.UnitType)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit!)
                    .ThenInclude(pu => pu!.UnitType)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.WeightUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.VolumeUnit)
            .Include(p => p.PrimaryVendor)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.BaseUnit)
            .Include(p => p.Unit!)
                .ThenInclude(u => u.PackagingUnit)
            .Include(p => p.PrimaryVendor)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}