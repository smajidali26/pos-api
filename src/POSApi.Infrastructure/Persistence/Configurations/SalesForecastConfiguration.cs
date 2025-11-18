using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class SalesForecastConfiguration : IEntityTypeConfiguration<SalesForecast>
{
    public void Configure(EntityTypeBuilder<SalesForecast> builder)
    {
        builder.ToTable("SalesForecast");

        builder.HasKey(sf => sf.Id);

        builder.Property(sf => sf.ForecastDate)
            .IsRequired();

        builder.Property(sf => sf.PredictedQuantity)
            .HasPrecision(18, 2);

        builder.Property(sf => sf.PredictedRevenue)
            .HasPrecision(18, 2);

        builder.Property(sf => sf.ConfidenceLevel)
            .HasPrecision(5, 4);

        builder.Property(sf => sf.ForecastMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sf => sf.Metadata)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, default(System.Text.Json.JsonSerializerOptions)),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(System.Text.Json.JsonSerializerOptions)) ?? new Dictionary<string, object>()
            );

        // Composite index on ProductId, StoreId, and ForecastDate
        builder.HasIndex(sf => new { sf.ProductId, sf.StoreId, sf.ForecastDate });

        // Index on CreatedAt for sorting
        builder.HasIndex(sf => sf.CreatedAt);

        // Index on ForecastMethod for filtering
        builder.HasIndex(sf => sf.ForecastMethod);

        // Relationship with Product
        builder.HasOne(sf => sf.Product)
            .WithMany()
            .HasForeignKey(sf => sf.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Store (nullable)
        builder.HasOne(sf => sf.Store)
            .WithMany()
            .HasForeignKey(sf => sf.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(sf => sf.DomainEvents);
    }
}
