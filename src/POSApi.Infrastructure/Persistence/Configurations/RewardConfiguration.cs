using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("Reward");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.PointsCost)
            .IsRequired();

        builder.Property(r => r.RewardType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Value)
            .HasPrecision(18, 2);

        builder.Property(r => r.ValidFrom)
            .IsRequired();

        builder.Property(r => r.ValidTo)
            .IsRequired();

        builder.Property(r => r.CurrentRedemptions)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.IsActive)
            .IsRequired();

        // Index on IsActive for filtering
        builder.HasIndex(r => r.IsActive);

        // Index on ValidFrom and ValidTo for validity checks
        builder.HasIndex(r => new { r.ValidFrom, r.ValidTo });

        // Index on RewardType for filtering
        builder.HasIndex(r => r.RewardType);

        // Index on PointsCost for filtering
        builder.HasIndex(r => r.PointsCost);

        // Relationship with Product (nullable)
        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}
