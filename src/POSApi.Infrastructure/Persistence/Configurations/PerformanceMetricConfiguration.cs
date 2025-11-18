using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        builder.ToTable("PerformanceMetrics");
        
        builder.HasKey(pm => pm.Id);
        
        builder.Property(pm => pm.PeriodType)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(pm => pm.TotalSales)
            .HasPrecision(18, 2);
            
        builder.Property(pm => pm.AverageOrderValue)
            .HasPrecision(18, 2);
            
        builder.Property(pm => pm.CommissionEarned)
            .HasPrecision(18, 2);
            
        builder.Property(pm => pm.RefundAmount)
            .HasPrecision(18, 2);
            
        builder.Property(pm => pm.RefundRate)
            .HasPrecision(5, 2);
            
        builder.Property(pm => pm.TotalHoursWorked)
            .HasPrecision(10, 2);
            
        builder.Property(pm => pm.TotalBreakHours)
            .HasPrecision(10, 2);
            
        builder.Property(pm => pm.AverageCustomerRating)
            .HasPrecision(3, 2);
            
        builder.Property(pm => pm.PerformanceScore)
            .HasPrecision(5, 2);
            
        builder.Property(pm => pm.PerformanceGrade)
            .HasMaxLength(10);
            
        builder.Property(pm => pm.Notes)
            .HasMaxLength(2000);
            
        builder.HasOne(pm => pm.EmployeeProfile)
            .WithMany()
            .HasForeignKey(pm => pm.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(pm => new { pm.EmployeeProfileId, pm.PeriodStart, pm.PeriodEnd });
        builder.HasIndex(pm => pm.PerformanceScore);
    }
}
