using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendor");
        
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(v => v.CompanyName)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(v => v.ContactPerson)
            .HasMaxLength(200);
            
        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(v => v.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(v => v.Address)
            .HasMaxLength(500);
            
        builder.Property(v => v.City)
            .HasMaxLength(100);
            
        builder.Property(v => v.State)
            .HasMaxLength(50);
            
        builder.Property(v => v.ZipCode)
            .HasMaxLength(20);
            
        builder.Property(v => v.Country)
            .HasMaxLength(100);
            
        builder.Property(v => v.TaxId)
            .HasMaxLength(50);
            
        builder.Property(v => v.Website)
            .HasMaxLength(255);
            
        builder.Property(v => v.Type)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(v => v.PaymentTerms)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(v => v.CreditLimit)
            .HasPrecision(18, 2);
            
        builder.Property(v => v.CurrentBalance)
            .HasPrecision(18, 2);
            
        builder.Property(v => v.Notes)
            .HasMaxLength(2000);
            
        builder.HasIndex(v => v.Name)
            .IsUnique();
            
        builder.HasIndex(v => v.Email)
            .IsUnique();
            
        builder.HasIndex(v => v.TaxId)
            .IsUnique()
            .HasFilter("[TaxId] IS NOT NULL AND [TaxId] != ''");
            
        // Ignore computed properties
        builder.Ignore(v => v.AvailableCredit);
        builder.Ignore(v => v.FullAddress);
        
        // Ignore domain events
        builder.Ignore(v => v.DomainEvents);
    }
}