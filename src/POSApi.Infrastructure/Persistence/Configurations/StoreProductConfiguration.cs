using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class StoreProductConfiguration : IEntityTypeConfiguration<StoreProduct>
{
    public void Configure(EntityTypeBuilder<StoreProduct> builder)
    {
        builder.ToTable("StoreProduct");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.StorePrice)
            .HasPrecision(18, 2);

        builder.Property(sp => sp.BinLocation)
            .HasMaxLength(100);

        // Composite unique index on StoreId and ProductId
        builder.HasIndex(sp => new { sp.StoreId, sp.ProductId })
            .IsUnique();

        // Index on IsAvailable for filtering
        builder.HasIndex(sp => sp.IsAvailable);

        // Relationship with Store
        builder.HasOne(sp => sp.Store)
            .WithMany(s => s.StoreProducts)
            .HasForeignKey(sp => sp.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Product
        builder.HasOne(sp => sp.Product)
            .WithMany()
            .HasForeignKey(sp => sp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed properties
        builder.Ignore(sp => sp.IsLowStock);
    }
}
