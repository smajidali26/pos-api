using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("Return");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.ReturnNumber)
            .IsUnique();

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.RefundMethod)
            .HasConversion<string>();

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.RefundAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.ReturnDate)
            .IsRequired();

        // Relationships
        builder.HasOne(r => r.OriginalOrder)
            .WithMany()
            .HasForeignKey(r => r.OriginalOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.ProcessedBy)
            .WithMany()
            .HasForeignKey(r => r.ProcessedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.ReturnItems)
            .WithOne(ri => ri.Return)
            .HasForeignKey(ri => ri.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.OriginalOrderId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.ProcessedByUserId);
        builder.HasIndex(r => r.ReturnDate);
        builder.HasIndex(r => r.Status);
    }
}