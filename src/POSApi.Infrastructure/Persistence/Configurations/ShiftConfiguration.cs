using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(s => s.Notes)
            .HasMaxLength(2000);
            
        builder.Property(s => s.TotalSales)
            .HasPrecision(18, 2);
            
        builder.HasOne(s => s.EmployeeProfile)
            .WithMany()
            .HasForeignKey(s => s.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(s => s.Store)
            .WithMany()
            .HasForeignKey(s => s.StoreId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasMany(s => s.Attendances)
            .WithOne()
            .HasForeignKey(a => a.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Ignore(s => s.DomainEvents);
    }
}
