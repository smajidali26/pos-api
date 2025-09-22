using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Description)
            .HasMaxLength(1000);
            
        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.Barcode)
            .HasMaxLength(50);
            
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
            
        builder.Property(p => p.Cost)
            .HasPrecision(18, 2);
            
        builder.Property(p => p.LastPurchaseCost)
            .HasPrecision(18, 2);
            
        builder.Property(p => p.VendorProductCode)
            .HasMaxLength(100);
        
        // UOM-related properties
        builder.Property(p => p.PricePerBaseUnit)
            .HasPrecision(18, 6);
            
        builder.Property(p => p.CostPerBaseUnit)
            .HasPrecision(18, 6);
            
        builder.HasIndex(p => p.SKU)
            .IsUnique();
            
        builder.HasIndex(p => p.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");
            
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(p => p.PrimaryVendor)
            .WithMany(v => v.Products)
            .HasForeignKey(p => p.PrimaryVendorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // UOM relationship
        builder.HasOne(p => p.Unit)
            .WithOne(pu => pu.Product)
            .HasForeignKey<ProductUnit>(pu => pu.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ignore computed properties
        builder.Ignore(p => p.NeedsReorder);
        builder.Ignore(p => p.IsLowStock);
        builder.Ignore(p => p.IsOutOfStock);
        builder.Ignore(p => p.HasPhysicalWeight);
        builder.Ignore(p => p.HasPhysicalVolume);
        builder.Ignore(p => p.IsWeightBased);
        builder.Ignore(p => p.IsVolumeBased);
        builder.Ignore(p => p.IsCountBased);
        
        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}