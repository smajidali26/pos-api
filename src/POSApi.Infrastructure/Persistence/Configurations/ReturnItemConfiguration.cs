using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> builder)
    {
        builder.ToTable("ReturnItem");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Quantity)
            .IsRequired();

        builder.Property(ri => ri.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(ri => ri.Reason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(ri => ri.Return)
            .WithMany(r => r.ReturnItems)
            .HasForeignKey(ri => ri.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.Product)
            .WithMany()
            .HasForeignKey(ri => ri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ri => ri.ReturnId);
        builder.HasIndex(ri => ri.ProductId);

        // Computed column for TotalRefund (if supported by database)
        // builder.Property(ri => ri.TotalRefund)
        //     .HasComputedColumnSql("[Quantity] * [UnitPrice]");
    }
}