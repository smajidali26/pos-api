using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class CommissionTransactionConfiguration : IEntityTypeConfiguration<CommissionTransaction>
{
    public void Configure(EntityTypeBuilder<CommissionTransaction> builder)
    {
        builder.ToTable("CommissionTransactions");
        
        builder.HasKey(ct => ct.Id);
        
        builder.Property(ct => ct.SaleAmount)
            .HasPrecision(18, 2);
            
        builder.Property(ct => ct.CommissionAmount)
            .HasPrecision(18, 2);
            
        builder.Property(ct => ct.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(ct => ct.PaymentReference)
            .HasMaxLength(100);
            
        builder.Property(ct => ct.Notes)
            .HasMaxLength(1000);
            
        builder.HasOne(ct => ct.EmployeeProfile)
            .WithMany()
            .HasForeignKey(ct => ct.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(ct => ct.Commission)
            .WithMany(c => c.Transactions)
            .HasForeignKey(ct => ct.CommissionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(ct => ct.Order)
            .WithMany()
            .HasForeignKey(ct => ct.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(ct => ct.OrderItem)
            .WithMany()
            .HasForeignKey(ct => ct.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(ct => new { ct.EmployeeProfileId, ct.TransactionDate });
        builder.HasIndex(ct => ct.Status);
    }
}
