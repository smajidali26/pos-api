using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class CustomerLoyaltyConfiguration : IEntityTypeConfiguration<CustomerLoyalty>
{
    public void Configure(EntityTypeBuilder<CustomerLoyalty> builder)
    {
        builder.ToTable("CustomerLoyalty");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.CurrentPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(cl => cl.LifetimePoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(cl => cl.LifetimeSpend)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(cl => cl.JoinDate)
            .IsRequired();

        builder.Property(cl => cl.LastActivityDate)
            .IsRequired();

        // Unique constraint on CustomerId
        builder.HasIndex(cl => cl.CustomerId)
            .IsUnique();

        // Index on CurrentTierId for filtering
        builder.HasIndex(cl => cl.CurrentTierId);

        // Index on JoinDate for reporting
        builder.HasIndex(cl => cl.JoinDate);

        // Index on LastActivityDate for reporting
        builder.HasIndex(cl => cl.LastActivityDate);

        // Relationship with Customer
        builder.HasOne(cl => cl.Customer)
            .WithMany()
            .HasForeignKey(cl => cl.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with CurrentTier (nullable)
        builder.HasOne(cl => cl.CurrentTier)
            .WithMany()
            .HasForeignKey(cl => cl.CurrentTierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure cascade delete for transactions
        builder.HasMany(cl => cl.Transactions)
            .WithOne(lt => lt.CustomerLoyalty)
            .HasForeignKey(lt => lt.CustomerLoyaltyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(cl => cl.DomainEvents);
    }
}
