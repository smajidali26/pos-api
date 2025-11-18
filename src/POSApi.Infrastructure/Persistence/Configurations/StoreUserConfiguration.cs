using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class StoreUserConfiguration : IEntityTypeConfiguration<StoreUser>
{
    public void Configure(EntityTypeBuilder<StoreUser> builder)
    {
        builder.ToTable("StoreUser");

        builder.HasKey(su => su.Id);

        builder.Property(su => su.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(su => su.AssignedDate)
            .IsRequired();

        // Composite index on StoreId and UserId for quick lookups
        builder.HasIndex(su => new { su.StoreId, su.UserId });

        // Index on IsActive for filtering
        builder.HasIndex(su => su.IsActive);

        // Relationship with Store
        builder.HasOne(su => su.Store)
            .WithMany(s => s.StoreUsers)
            .HasForeignKey(su => su.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(su => su.User)
            .WithMany()
            .HasForeignKey(su => su.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
