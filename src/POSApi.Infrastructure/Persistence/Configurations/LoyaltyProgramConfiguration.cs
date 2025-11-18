using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class LoyaltyProgramConfiguration : IEntityTypeConfiguration<LoyaltyProgram>
{
    public void Configure(EntityTypeBuilder<LoyaltyProgram> builder)
    {
        builder.ToTable("LoyaltyProgram");

        builder.HasKey(lp => lp.Id);

        builder.Property(lp => lp.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(lp => lp.Description)
            .HasMaxLength(2000);

        builder.Property(lp => lp.PointsPerDollar)
            .HasPrecision(5, 2);

        builder.Property(lp => lp.MinimumPurchaseAmount)
            .HasPrecision(18, 2);

        builder.Property(lp => lp.PointsExpiryDays)
            .IsRequired();

        builder.Property(lp => lp.IsActive)
            .IsRequired();

        // Index on IsActive for filtering
        builder.HasIndex(lp => lp.IsActive);

        // Ignore domain events
        builder.Ignore(lp => lp.DomainEvents);
    }
}
