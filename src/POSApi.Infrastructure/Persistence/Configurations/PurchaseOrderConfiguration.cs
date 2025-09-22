using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrder");
        
        builder.HasKey(po => po.Id);
        
        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(po => po.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(po => po.SubTotal)
            .HasPrecision(18, 2);
            
        builder.Property(po => po.TaxAmount)
            .HasPrecision(18, 2);
            
        builder.Property(po => po.ShippingCost)
            .HasPrecision(18, 2);
            
        builder.Property(po => po.DiscountAmount)
            .HasPrecision(18, 2);
            
        builder.Property(po => po.TotalAmount)
            .HasPrecision(18, 2);
            
        builder.Property(po => po.Notes)
            .HasMaxLength(2000);
            
        builder.HasIndex(po => po.OrderNumber)
            .IsUnique();
            
        builder.HasOne(po => po.Vendor)
            .WithMany(v => v.PurchaseOrders)
            .HasForeignKey(po => po.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(po => po.CreatedBy)
            .WithMany()
            .HasForeignKey(po => po.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(po => po.Items)
            .WithOne(poi => poi.PurchaseOrder)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ignore computed properties
        builder.Ignore(po => po.IsOverdue);
        
        // Ignore domain events
        builder.Ignore(po => po.DomainEvents);
    }
}