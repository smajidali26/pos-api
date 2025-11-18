using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Store");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ZipCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(s => s.Email)
            .HasMaxLength(100);

        builder.Property(s => s.TimeZone)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.TaxRate)
            .HasPrecision(5, 4);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        builder.Property(s => s.StoreType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Unique constraint on StoreCode
        builder.HasIndex(s => s.Code)
            .IsUnique();

        // Index on StoreType and IsActive for filtering
        builder.HasIndex(s => new { s.StoreType, s.IsActive });

        // Index on ManagerUserId
        builder.HasIndex(s => s.ManagerUserId);

        // Self-referencing relationship for ParentStore
        builder.HasOne(s => s.ParentStore)
            .WithMany(s => s.ChildStores)
            .HasForeignKey(s => s.ParentStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Manager (User)
        builder.HasOne(s => s.Manager)
            .WithMany()
            .HasForeignKey(s => s.ManagerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(s => s.DomainEvents);

        // Ignore computed property
        builder.Ignore(s => s.FullAddress);
    }
}
