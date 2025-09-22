using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder.ToTable("UnitType");
        
        builder.HasKey(ut => ut.Id);
        
        builder.Property(ut => ut.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(ut => ut.Description)
            .HasMaxLength(500);
            
        builder.Property(ut => ut.SortOrder)
            .HasDefaultValue(0);
            
        builder.HasIndex(ut => ut.Name)
            .IsUnique();
            
        builder.HasIndex(ut => ut.SortOrder);
        
        // Relationships
        builder.HasMany(ut => ut.UnitsOfMeasure)
            .WithOne(uom => uom.UnitType)
            .HasForeignKey(uom => uom.UnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitOfMeasure");
        
        builder.HasKey(uom => uom.Id);
        
        builder.Property(uom => uom.Code)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(uom => uom.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(uom => uom.Symbol)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(uom => uom.ConversionFactorToBase)
            .HasPrecision(18, 6)
            .HasDefaultValue(1.0m);
            
        builder.Property(uom => uom.Abbreviation)
            .HasMaxLength(10);
            
        builder.Property(uom => uom.SortOrder)
            .HasDefaultValue(0);
            
        builder.HasIndex(uom => uom.Code)
            .IsUnique();
            
        builder.HasIndex(uom => new { uom.UnitTypeId, uom.SortOrder });
        builder.HasIndex(uom => uom.IsBaseUnit);
        
        // Self-referencing relationship for base unit
        builder.HasOne(uom => uom.BaseUnit)
            .WithMany(uom => uom.DerivedUnits)
            .HasForeignKey(uom => uom.BaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relationship with UnitType
        builder.HasOne(uom => uom.UnitType)
            .WithMany(ut => ut.UnitsOfMeasure)
            .HasForeignKey(uom => uom.UnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        builder.ToTable("ProductUnit");
        
        builder.HasKey(pu => pu.Id);
        
        builder.Property(pu => pu.BaseQuantity)
            .HasPrecision(18, 6)
            .HasDefaultValue(1.0m);
            
        builder.Property(pu => pu.PackagingQuantity)
            .HasPrecision(18, 6);
            
        builder.Property(pu => pu.DisplayName)
            .HasMaxLength(200);
            
        builder.Property(pu => pu.Weight)
            .HasPrecision(18, 6);
            
        builder.Property(pu => pu.Volume)
            .HasPrecision(18, 6);
        
        // Indexes
        builder.HasIndex(pu => pu.ProductId)
            .IsUnique(); // One ProductUnit per Product
            
        builder.HasIndex(pu => pu.BaseUnitId);
        builder.HasIndex(pu => pu.PackagingUnitId);
        
        // Relationships
        builder.HasOne(pu => pu.Product)
            .WithOne(p => p.Unit)
            .HasForeignKey<ProductUnit>(pu => pu.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(pu => pu.BaseUnit)
            .WithMany(uom => uom.BaseProductUnits)
            .HasForeignKey(pu => pu.BaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(pu => pu.PackagingUnit)
            .WithMany(uom => uom.PackagingProductUnits)
            .HasForeignKey(pu => pu.PackagingUnitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(pu => pu.WeightUnit)
            .WithMany()
            .HasForeignKey(pu => pu.WeightUnitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(pu => pu.VolumeUnit)
            .WithMany()
            .HasForeignKey(pu => pu.VolumeUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}