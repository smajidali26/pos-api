using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class RewardRedemptionConfiguration : IEntityTypeConfiguration<RewardRedemption>
{
    public void Configure(EntityTypeBuilder<RewardRedemption> builder)
    {
        builder.ToTable("RewardRedemption");

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.PointsUsed)
            .IsRequired();

        builder.Property(rr => rr.RedeemedAt)
            .IsRequired();

        builder.Property(rr => rr.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rr => rr.ExpiryDate)
            .IsRequired();

        // Index on RewardId for lookups
        builder.HasIndex(rr => rr.RewardId);

        // Index on CustomerLoyaltyId for lookups
        builder.HasIndex(rr => rr.CustomerLoyaltyId);

        // Index on RedeemedAt for sorting
        builder.HasIndex(rr => rr.RedeemedAt);

        // Index on IsUsed for filtering
        builder.HasIndex(rr => rr.IsUsed);

        // Index on ExpiryDate for expiry checks
        builder.HasIndex(rr => rr.ExpiryDate);

        // Composite index for common queries
        builder.HasIndex(rr => new { rr.CustomerLoyaltyId, rr.IsUsed });

        // Relationship with Reward
        builder.HasOne(rr => rr.Reward)
            .WithMany()
            .HasForeignKey(rr => rr.RewardId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with CustomerLoyalty
        builder.HasOne(rr => rr.CustomerLoyalty)
            .WithMany()
            .HasForeignKey(rr => rr.CustomerLoyaltyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Order (nullable)
        builder.HasOne(rr => rr.Order)
            .WithMany()
            .HasForeignKey(rr => rr.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
