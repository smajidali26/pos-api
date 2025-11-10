using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(o => o.SubtotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.CashAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.CardAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ChangeAmount)
            .HasPrecision(18, 2);
            
        builder.Property(o => o.TaxAmount)
            .HasPrecision(18, 2);
            
        builder.Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);
            
        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);
            
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(o => o.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(o => o.Notes)
            .HasMaxLength(1000);
            
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();
            
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(o => o.Cashier)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ignore domain events
        builder.Ignore(o => o.DomainEvents);
    }
}