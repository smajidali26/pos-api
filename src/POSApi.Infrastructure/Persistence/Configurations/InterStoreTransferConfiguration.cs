using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class InterStoreTransferConfiguration : IEntityTypeConfiguration<InterStoreTransfer>
{
    public void Configure(EntityTypeBuilder<InterStoreTransfer> builder)
    {
        builder.ToTable("InterStoreTransfer");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.RequestDate)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(2000);

        builder.Property(t => t.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(t => t.CancellationReason)
            .HasMaxLength(1000);

        // Unique constraint on TransferNumber
        builder.HasIndex(t => t.TransferNumber)
            .IsUnique();

        // Index on Status for filtering
        builder.HasIndex(t => t.Status);

        // Index on RequestDate for sorting
        builder.HasIndex(t => t.RequestDate);

        // Composite index for common queries
        builder.HasIndex(t => new { t.FromStoreId, t.Status });
        builder.HasIndex(t => new { t.ToStoreId, t.Status });

        // Relationship with FromStore
        builder.HasOne(t => t.FromStore)
            .WithMany(s => s.TransfersFrom)
            .HasForeignKey(t => t.FromStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with ToStore
        builder.HasOne(t => t.ToStore)
            .WithMany(s => s.TransfersTo)
            .HasForeignKey(t => t.ToStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with RequestedBy user
        builder.HasOne(t => t.RequestedBy)
            .WithMany()
            .HasForeignKey(t => t.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with ApprovedBy user (nullable)
        builder.HasOne(t => t.ApprovedBy)
            .WithMany()
            .HasForeignKey(t => t.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure cascade delete for transfer items
        builder.HasMany(t => t.TransferItems)
            .WithOne(ti => ti.Transfer)
            .HasForeignKey(ti => ti.TransferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}
