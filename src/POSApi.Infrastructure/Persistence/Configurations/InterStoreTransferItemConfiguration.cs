using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class InterStoreTransferItemConfiguration : IEntityTypeConfiguration<InterStoreTransferItem>
{
    public void Configure(EntityTypeBuilder<InterStoreTransferItem> builder)
    {
        builder.ToTable("InterStoreTransferItem");

        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.RequestedQuantity)
            .IsRequired();

        builder.Property(ti => ti.ApprovedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ti => ti.ShippedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ti => ti.ReceivedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ti => ti.UnitCost)
            .HasPrecision(18, 2);

        // Index on TransferId for quick lookups
        builder.HasIndex(ti => ti.TransferId);

        // Index on ProductId for reporting
        builder.HasIndex(ti => ti.ProductId);

        // Relationship with Transfer (configured in InterStoreTransferConfiguration)
        builder.HasOne(ti => ti.Transfer)
            .WithMany(t => t.TransferItems)
            .HasForeignKey(ti => ti.TransferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Product
        builder.HasOne(ti => ti.Product)
            .WithMany()
            .HasForeignKey(ti => ti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed properties
        builder.Ignore(ti => ti.TotalCost);
    }
}
