using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class ProductABCClassificationConfiguration : IEntityTypeConfiguration<ProductABCClassification>
{
    public void Configure(EntityTypeBuilder<ProductABCClassification> builder)
    {
        builder.ToTable("ProductABCClassification");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Classification)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(p => p.AnnualVolume)
            .HasPrecision(18, 2);

        builder.Property(p => p.AnnualRevenue)
            .HasPrecision(18, 2);

        builder.Property(p => p.ContributionPercentage)
            .HasPrecision(5, 4);

        builder.Property(p => p.CumulativePercentage)
            .HasPrecision(5, 4);

        builder.Property(p => p.Rank)
            .IsRequired();

        builder.Property(p => p.LastCalculatedAt)
            .IsRequired();

        // Composite unique index on ProductId and StoreId
        builder.HasIndex(p => new { p.ProductId, p.StoreId })
            .IsUnique();

        // Index on Classification for filtering
        builder.HasIndex(p => p.Classification);

        // Index on LastCalculatedAt for sorting
        builder.HasIndex(p => p.LastCalculatedAt);

        // Index on Rank for sorting
        builder.HasIndex(p => p.Rank);

        // Relationship with Product
        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Store (nullable)
        builder.HasOne(p => p.Store)
            .WithMany()
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}
