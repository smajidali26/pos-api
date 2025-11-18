using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class CustomerTierConfiguration : IEntityTypeConfiguration<CustomerTier>
{
    public void Configure(EntityTypeBuilder<CustomerTier> builder)
    {
        builder.ToTable("CustomerTier");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ct => ct.MinSpend)
            .HasPrecision(18, 2);

        builder.Property(ct => ct.MinPoints)
            .IsRequired();

        builder.Property(ct => ct.BenefitMultiplier)
            .HasPrecision(5, 2);

        builder.Property(ct => ct.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.Property(ct => ct.Color)
            .HasMaxLength(50);

        builder.Property(ct => ct.SortOrder)
            .IsRequired();

        // Unique constraint on Name
        builder.HasIndex(ct => ct.Name)
            .IsUnique();

        // Index on MinPoints for eligibility queries
        builder.HasIndex(ct => ct.MinPoints);

        // Index on SortOrder for sorting
        builder.HasIndex(ct => ct.SortOrder);

        // Ignore domain events
        builder.Ignore(ct => ct.DomainEvents);
    }
}
