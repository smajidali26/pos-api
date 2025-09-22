using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItem");
        
        builder.HasKey(poi => poi.Id);
        
        builder.Property(poi => poi.Quantity)
            .IsRequired();
            
        builder.Property(poi => poi.ReceivedQuantity)
            .IsRequired();
            
        builder.Property(poi => poi.UnitCost)
            .HasPrecision(18, 2);
            
        builder.HasOne(poi => poi.Product)
            .WithMany()
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Ignore computed properties
        builder.Ignore(poi => poi.TotalCost);
        builder.Ignore(poi => poi.IsFullyReceived);
        builder.Ignore(poi => poi.PendingQuantity);
    }
}