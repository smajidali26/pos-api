using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class UnitTypeRepository : Repository<UnitType>, IUnitTypeRepository
{
    public UnitTypeRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<UnitType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ut => ut.UnitsOfMeasure.Where(uom => uom.IsActive))
            .FirstOrDefaultAsync(ut => ut.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<UnitType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ut => ut.IsActive)
            .Include(ut => ut.UnitsOfMeasure.Where(uom => uom.IsActive))
            .OrderBy(ut => ut.SortOrder)
            .ThenBy(ut => ut.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitType>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ut => ut.UnitsOfMeasure)
            .OrderBy(ut => ut.SortOrder)
            .ThenBy(ut => ut.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<UnitType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ut => ut.UnitsOfMeasure)
            .OrderBy(ut => ut.SortOrder)
            .ThenBy(ut => ut.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<UnitType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ut => ut.UnitsOfMeasure)
            .FirstOrDefaultAsync(ut => ut.Id == id, cancellationToken);
    }
}

public class UnitOfMeasureRepository : Repository<UnitOfMeasure>, IUnitOfMeasureRepository
{
    public UnitOfMeasureRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .FirstOrDefaultAsync(uom => uom.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetByUnitTypeAsync(Guid unitTypeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.UnitTypeId == unitTypeId)
            .OrderBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetByUnitTypeNameAsync(string unitTypeName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.UnitType.Name == unitTypeName)
            .OrderBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetBaseUnitsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Where(uom => uom.IsBaseUnit)
            .OrderBy(uom => uom.UnitType.SortOrder)
            .ThenBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.IsActive)
            .OrderBy(uom => uom.UnitType.SortOrder)
            .ThenBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetActiveByUnitTypeAsync(Guid unitTypeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.UnitTypeId == unitTypeId && uom.IsActive)
            .OrderBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.Name.ToLower().Contains(term) ||
                         uom.Code.ToLower().Contains(term) ||
                         uom.Symbol.ToLower().Contains(term) ||
                         (uom.Abbreviation != null && uom.Abbreviation.ToLower().Contains(term)))
            .OrderBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetOrderedByTypeAsync(Guid unitTypeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.UnitTypeId == unitTypeId)
            .OrderBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<UnitOfMeasure>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .OrderBy(uom => uom.UnitType.SortOrder)
            .ThenBy(uom => uom.SortOrder)
            .ThenBy(uom => uom.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uom => uom.UnitType)
            .Include(uom => uom.BaseUnit)
            .Include(uom => uom.DerivedUnits)
            .FirstOrDefaultAsync(uom => uom.Id == id, cancellationToken);
    }
}

public class ProductUnitRepository : Repository<ProductUnit>, IProductUnitRepository
{
    public ProductUnitRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<ProductUnit?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
                .ThenInclude(uom => uom.UnitType)
            .Include(pu => pu.PackagingUnit)
                .ThenInclude(uom => uom!.UnitType)
            .Include(pu => pu.WeightUnit)
            .Include(pu => pu.VolumeUnit)
            .FirstOrDefaultAsync(pu => pu.ProductId == productId, cancellationToken);
    }

    public async Task<IEnumerable<ProductUnit>> GetByBaseUnitAsync(Guid baseUnitId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
            .Include(pu => pu.PackagingUnit)
            .Where(pu => pu.BaseUnitId == baseUnitId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductUnit>> GetByPackagingUnitAsync(Guid packagingUnitId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
            .Include(pu => pu.PackagingUnit)
            .Where(pu => pu.PackagingUnitId == packagingUnitId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductUnit>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
            .Include(pu => pu.PackagingUnit)
            .Where(pu => pu.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductUnit>> GetWithPhysicalPropertiesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
            .Include(pu => pu.PackagingUnit)
            .Include(pu => pu.WeightUnit)
            .Include(pu => pu.VolumeUnit)
            .Where(pu => pu.Weight.HasValue || pu.Volume.HasValue)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<ProductUnit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
                .ThenInclude(uom => uom.UnitType)
            .Include(pu => pu.PackagingUnit)
                .ThenInclude(uom => uom!.UnitType)
            .Include(pu => pu.WeightUnit)
            .Include(pu => pu.VolumeUnit)
            .ToListAsync(cancellationToken);
    }

    public override async Task<ProductUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pu => pu.Product)
            .Include(pu => pu.BaseUnit)
                .ThenInclude(uom => uom.UnitType)
            .Include(pu => pu.PackagingUnit)
                .ThenInclude(uom => uom!.UnitType)
            .Include(pu => pu.WeightUnit)
            .Include(pu => pu.VolumeUnit)
            .FirstOrDefaultAsync(pu => pu.Id == id, cancellationToken);
    }
}