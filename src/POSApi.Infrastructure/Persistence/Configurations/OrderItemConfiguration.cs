using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItem");
        
        builder.HasKey(oi => oi.Id);
        
        builder.Property(oi => oi.Quantity)
            .IsRequired();
            
        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);
            
        builder.Property(oi => oi.DiscountAmount)
            .HasPrecision(18, 2);
            
        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Ignore computed property
        builder.Ignore(oi => oi.TotalPrice);
    }
}