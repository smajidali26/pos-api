using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotion");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.DiscountType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.MinimumPurchaseAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.MaximumDiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.CouponCode)
            .HasMaxLength(50);

        builder.Property(p => p.Target)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PromotionProducts)
            .WithOne(pp => pp.Promotion)
            .HasForeignKey(pp => pp.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PromotionCategories)
            .WithOne(pc => pc.Promotion)
            .HasForeignKey(pc => pc.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PromotionUsages)
            .WithOne(pu => pu.Promotion)
            .HasForeignKey(pu => pu.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.CouponCode)
            .IsUnique()
            .HasFilter("[CouponCode] IS NOT NULL");

        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.StartDate);
        builder.HasIndex(p => p.EndDate);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.CreatedByUserId);
        builder.HasIndex(p => new { p.StartDate, p.EndDate, p.IsActive });
    }
}

public class PromotionProductConfiguration : IEntityTypeConfiguration<PromotionProduct>
{
    public void Configure(EntityTypeBuilder<PromotionProduct> builder)
    {
        builder.ToTable("PromotionProduct");

        builder.HasKey(pp => pp.Id);

        builder.HasOne(pp => pp.Promotion)
            .WithMany(p => p.PromotionProducts)
            .HasForeignKey(pp => pp.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Product)
            .WithMany()
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pp => pp.PromotionId);
        builder.HasIndex(pp => pp.ProductId);
        builder.HasIndex(pp => new { pp.PromotionId, pp.ProductId })
            .IsUnique();
    }
}

public class PromotionCategoryConfiguration : IEntityTypeConfiguration<PromotionCategory>
{
    public void Configure(EntityTypeBuilder<PromotionCategory> builder)
    {
        builder.ToTable("PromotionCategory");

        builder.HasKey(pc => pc.Id);

        builder.HasOne(pc => pc.Promotion)
            .WithMany(p => p.PromotionCategories)
            .HasForeignKey(pc => pc.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Category)
            .WithMany()
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pc => pc.PromotionId);
        builder.HasIndex(pc => pc.CategoryId);
        builder.HasIndex(pc => new { pc.PromotionId, pc.CategoryId })
            .IsUnique();
    }
}

public class PromotionUsageConfiguration : IEntityTypeConfiguration<PromotionUsage>
{
    public void Configure(EntityTypeBuilder<PromotionUsage> builder)
    {
        builder.ToTable("PromotionUsage");

        builder.HasKey(pu => pu.Id);

        builder.Property(pu => pu.DiscountAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(pu => pu.UsedAt)
            .IsRequired();

        builder.HasOne(pu => pu.Promotion)
            .WithMany(p => p.PromotionUsages)
            .HasForeignKey(pu => pu.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.Order)
            .WithMany()
            .HasForeignKey(pu => pu.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pu => pu.Customer)
            .WithMany()
            .HasForeignKey(pu => pu.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(pu => pu.PromotionId);
        builder.HasIndex(pu => pu.OrderId);
        builder.HasIndex(pu => pu.CustomerId);
        builder.HasIndex(pu => pu.UsedAt);
    }
}