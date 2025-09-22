using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Represents a unit type/category (Weight, Volume, Length, etc.)
/// </summary>
public class UnitType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    // Navigation properties
    public ICollection<UnitOfMeasure> UnitsOfMeasure { get; private set; } = new List<UnitOfMeasure>();

    private UnitType() { } // For EF Core

    public UnitType(string name, string description, int sortOrder = 0)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
    }

    public void UpdateDetails(string name, string description, int sortOrder)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}

/// <summary>
/// Represents a specific unit of measure (kg, ml, piece, etc.)
/// </summary>
public class UnitOfMeasure : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = string.Empty;
    public Guid UnitTypeId { get; private set; }
    public UnitType UnitType { get; private set; } = null!;
    public decimal ConversionFactorToBase { get; private set; } = 1m;
    public Guid? BaseUnitId { get; private set; }
    public UnitOfMeasure? BaseUnit { get; private set; }
    public bool IsBaseUnit { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }
    public string? Abbreviation { get; private set; }

    // Navigation properties
    public ICollection<UnitOfMeasure> DerivedUnits { get; private set; } = new List<UnitOfMeasure>();
    public ICollection<ProductUnit> BaseProductUnits { get; private set; } = new List<ProductUnit>();
    public ICollection<ProductUnit> PackagingProductUnits { get; private set; } = new List<ProductUnit>();

    private UnitOfMeasure() { } // For EF Core

    public UnitOfMeasure(string code, string name, string symbol, Guid unitTypeId, 
                        decimal conversionFactorToBase = 1m, Guid? baseUnitId = null, 
                        string? abbreviation = null, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Unit code cannot be empty", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Unit name cannot be empty", nameof(name));
        if (conversionFactorToBase <= 0)
            throw new ArgumentException("Conversion factor must be positive", nameof(conversionFactorToBase));

        Code = code.ToUpperInvariant();
        Name = name;
        Symbol = symbol;
        UnitTypeId = unitTypeId;
        ConversionFactorToBase = conversionFactorToBase;
        BaseUnitId = baseUnitId;
        IsBaseUnit = baseUnitId == null;
        Abbreviation = abbreviation;
        SortOrder = sortOrder;
    }

    public void UpdateDetails(string name, string symbol, string? abbreviation, int sortOrder)
    {
        Name = name;
        Symbol = symbol;
        Abbreviation = abbreviation;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }

    public void UpdateConversionFactor(decimal conversionFactorToBase)
    {
        if (conversionFactorToBase <= 0)
            throw new ArgumentException("Conversion factor must be positive", nameof(conversionFactorToBase));

        ConversionFactorToBase = conversionFactorToBase;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Convert a quantity from this unit to the base unit
    /// </summary>
    public decimal ConvertToBaseUnit(decimal quantity)
    {
        return quantity * ConversionFactorToBase;
    }

    /// <summary>
    /// Convert a quantity from the base unit to this unit
    /// </summary>
    public decimal ConvertFromBaseUnit(decimal baseQuantity)
    {
        return baseQuantity / ConversionFactorToBase;
    }

    /// <summary>
    /// Convert a quantity from this unit to another unit of the same type
    /// </summary>
    public decimal ConvertTo(decimal quantity, UnitOfMeasure targetUnit)
    {
        if (UnitTypeId != targetUnit.UnitTypeId)
            throw new InvalidOperationException($"Cannot convert between different unit types");

        if (Id == targetUnit.Id)
            return quantity;

        // Convert to base unit first, then to target unit
        var baseQuantity = ConvertToBaseUnit(quantity);
        return targetUnit.ConvertFromBaseUnit(baseQuantity);
    }
}

/// <summary>
/// Represents product-specific unit configuration
/// </summary>
public class ProductUnit : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid BaseUnitId { get; private set; }
    public UnitOfMeasure BaseUnit { get; private set; } = null!;
    public decimal BaseQuantity { get; private set; } = 1m;
    public Guid? PackagingUnitId { get; private set; }
    public UnitOfMeasure? PackagingUnit { get; private set; }
    public decimal? PackagingQuantity { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Physical properties
    public decimal? Weight { get; private set; }
    public Guid? WeightUnitId { get; private set; }
    public UnitOfMeasure? WeightUnit { get; private set; }
    public decimal? Volume { get; private set; }
    public Guid? VolumeUnitId { get; private set; }
    public UnitOfMeasure? VolumeUnit { get; private set; }

    private ProductUnit() { } // For EF Core

    public ProductUnit(Guid productId, Guid baseUnitId, decimal baseQuantity = 1m,
                      Guid? packagingUnitId = null, decimal? packagingQuantity = null)
    {
        if (baseQuantity <= 0)
            throw new ArgumentException("Base quantity must be positive", nameof(baseQuantity));

        if (packagingUnitId.HasValue && packagingQuantity.HasValue)
        {
            if (packagingQuantity.Value <= 0)
                throw new ArgumentException("Packaging quantity must be positive", nameof(packagingQuantity));
        }

        ProductId = productId;
        BaseUnitId = baseUnitId;
        BaseQuantity = baseQuantity;
        PackagingUnitId = packagingUnitId;
        PackagingQuantity = packagingQuantity;
        
        GenerateDisplayName();
    }

    public void UpdateBaseUnit(Guid baseUnitId, decimal baseQuantity)
    {
        if (baseQuantity <= 0)
            throw new ArgumentException("Base quantity must be positive", nameof(baseQuantity));

        BaseUnitId = baseUnitId;
        BaseQuantity = baseQuantity;
        GenerateDisplayName();
        SetUpdatedAt();
    }

    public void UpdatePackaging(Guid? packagingUnitId, decimal? packagingQuantity)
    {
        if (packagingUnitId.HasValue && packagingQuantity.HasValue)
        {
            if (packagingQuantity.Value <= 0)
                throw new ArgumentException("Packaging quantity must be positive", nameof(packagingQuantity));
        }

        PackagingUnitId = packagingUnitId;
        PackagingQuantity = packagingQuantity;
        GenerateDisplayName();
        SetUpdatedAt();
    }

    public void UpdatePhysicalProperties(decimal? weight = null, Guid? weightUnitId = null,
                                       decimal? volume = null, Guid? volumeUnitId = null)
    {
        if (weight.HasValue && weight.Value <= 0)
            throw new ArgumentException("Weight must be positive", nameof(weight));

        if (volume.HasValue && volume.Value <= 0)
            throw new ArgumentException("Volume must be positive", nameof(volume));

        Weight = weight;
        WeightUnitId = weightUnitId;
        Volume = volume;
        VolumeUnitId = volumeUnitId;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    private void GenerateDisplayName()
    {
        if (!HasPackaging)
        {
            DisplayName = BaseQuantity == 1m ? "{BaseUnit}" : $"{BaseQuantity} {{BaseUnit}}";
        }
        else
        {
            DisplayName = $"{PackagingQuantity} {{PackagingUnit}}";
        }
    }

    // Computed properties
    public bool HasPackaging => PackagingUnitId.HasValue && PackagingQuantity.HasValue;
    public bool HasPhysicalWeight => Weight.HasValue && WeightUnitId.HasValue;
    public bool HasPhysicalVolume => Volume.HasValue && VolumeUnitId.HasValue;
}