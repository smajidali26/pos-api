using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("Commissions");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(c => c.CommissionType)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(c => c.CommissionBasis)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(c => c.Rate)
            .HasPrecision(18, 4);
            
        builder.Property(c => c.MinimumSaleAmount)
            .HasPrecision(18, 2);
            
        builder.Property(c => c.MaximumCommission)
            .HasPrecision(18, 2);
            
        builder.Property(c => c.Role)
            .HasMaxLength(50);
            
        builder.HasOne(c => c.EmployeeProfile)
            .WithMany()
            .HasForeignKey(c => c.EmployeeProfileId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Commission)
            .HasForeignKey(t => t.CommissionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Ignore(c => c.DomainEvents);
    }
}
