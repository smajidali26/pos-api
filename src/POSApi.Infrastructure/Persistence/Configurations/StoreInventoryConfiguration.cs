using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class StoreInventoryConfiguration : IEntityTypeConfiguration<StoreInventory>
{
    public void Configure(EntityTypeBuilder<StoreInventory> builder)
    {
        builder.ToTable("StoreInventory");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.MinStockLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(si => si.MaxStockLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(si => si.ReorderPoint)
            .IsRequired()
            .HasDefaultValue(0);

        // Composite unique index on StoreId and ProductId
        builder.HasIndex(si => new { si.StoreId, si.ProductId })
            .IsUnique();

        // Index on LastRestockedAt for reporting
        builder.HasIndex(si => si.LastRestockedAt);

        // Index on LastSoldAt for reporting
        builder.HasIndex(si => si.LastSoldAt);

        // Composite index for low stock queries
        builder.HasIndex(si => new { si.Quantity, si.MinStockLevel });

        // Relationship with Store
        builder.HasOne(si => si.Store)
            .WithMany(s => s.StoreInventories)
            .HasForeignKey(si => si.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Product
        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed properties
        builder.Ignore(si => si.IsLowStock);
        builder.Ignore(si => si.NeedsReorder);
        builder.Ignore(si => si.IsOutOfStock);
        builder.Ignore(si => si.IsOverstocked);
    }
}
