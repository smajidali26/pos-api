using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.ToTable("LoyaltyTransaction");

        builder.HasKey(lt => lt.Id);

        builder.Property(lt => lt.PointsEarned)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(lt => lt.PointsRedeemed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(lt => lt.BalanceBefore)
            .IsRequired();

        builder.Property(lt => lt.BalanceAfter)
            .IsRequired();

        builder.Property(lt => lt.TransactionDate)
            .IsRequired();

        builder.Property(lt => lt.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(lt => lt.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Index on CustomerLoyaltyId for quick lookups
        builder.HasIndex(lt => lt.CustomerLoyaltyId);

        // Index on TransactionDate for sorting
        builder.HasIndex(lt => lt.TransactionDate);

        // Index on TransactionType for filtering
        builder.HasIndex(lt => lt.TransactionType);

        // Index on ExpiryDate for expiry checks
        builder.HasIndex(lt => lt.ExpiryDate);

        // Composite index for expiring points queries
        builder.HasIndex(lt => new { lt.TransactionType, lt.ExpiryDate });

        // Relationship with CustomerLoyalty (configured in CustomerLoyaltyConfiguration)
        builder.HasOne(lt => lt.CustomerLoyalty)
            .WithMany(cl => cl.Transactions)
            .HasForeignKey(lt => lt.CustomerLoyaltyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Order (nullable)
        builder.HasOne(lt => lt.Order)
            .WithMany()
            .HasForeignKey(lt => lt.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
