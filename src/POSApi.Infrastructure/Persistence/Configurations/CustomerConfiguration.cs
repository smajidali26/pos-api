using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(c => c.Address)
            .HasMaxLength(500);
            
        builder.Property(c => c.City)
            .HasMaxLength(100);
            
        builder.Property(c => c.State)
            .HasMaxLength(50);
            
        builder.Property(c => c.ZipCode)
            .HasMaxLength(10);
            
        builder.Property(c => c.LoyaltyPoints)
            .HasPrecision(18, 2);
            
        builder.HasIndex(c => c.Email)
            .IsUnique();
            
        // Ignore computed property
        builder.Ignore(c => c.FullName);
        
        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}