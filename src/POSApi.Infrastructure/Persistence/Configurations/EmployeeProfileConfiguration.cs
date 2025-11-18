using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.ToTable("EmployeeProfiles");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(e => e.EmploymentType)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(e => e.Department)
            .HasMaxLength(100);
            
        builder.Property(e => e.JobTitle)
            .HasMaxLength(100);
            
        builder.Property(e => e.EmergencyContactName)
            .HasMaxLength(200);
            
        builder.Property(e => e.EmergencyContactPhone)
            .HasMaxLength(20);
            
        builder.Property(e => e.Address)
            .HasMaxLength(500);
            
        builder.Property(e => e.City)
            .HasMaxLength(100);
            
        builder.Property(e => e.State)
            .HasMaxLength(100);
            
        builder.Property(e => e.ZipCode)
            .HasMaxLength(20);
            
        builder.Property(e => e.Country)
            .HasMaxLength(100);
            
        builder.Property(e => e.HourlyRate)
            .HasPrecision(18, 2);
            
        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique();
            
        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<EmployeeProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.Ignore(e => e.DomainEvents);
    }
}
