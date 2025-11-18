using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class InventoryTurnoverConfiguration : IEntityTypeConfiguration<InventoryTurnover>
{
    public void Configure(EntityTypeBuilder<InventoryTurnover> builder)
    {
        builder.ToTable("InventoryTurnover");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.CalculationDate)
            .IsRequired();

        builder.Property(it => it.PeriodDays)
            .IsRequired();

        builder.Property(it => it.TurnoverRatio)
            .HasPrecision(18, 4);

        builder.Property(it => it.DaysToSell)
            .HasPrecision(18, 2);

        builder.Property(it => it.AverageCOGS)
            .HasPrecision(18, 2);

        builder.Property(it => it.AverageInventoryValue)
            .HasPrecision(18, 2);

        builder.Property(it => it.Classification)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Composite index on ProductId and StoreId
        builder.HasIndex(it => new { it.ProductId, it.StoreId });

        // Index on Classification for filtering
        builder.HasIndex(it => it.Classification);

        // Index on CalculationDate for sorting
        builder.HasIndex(it => it.CalculationDate);

        // Relationship with Product
        builder.HasOne(it => it.Product)
            .WithMany()
            .HasForeignKey(it => it.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Store (nullable)
        builder.HasOne(it => it.Store)
            .WithMany()
            .HasForeignKey(it => it.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
