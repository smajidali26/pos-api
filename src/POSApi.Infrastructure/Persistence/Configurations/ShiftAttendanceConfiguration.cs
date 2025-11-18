using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSApi.Domain.Entities;

namespace POSApi.Infrastructure.Persistence.Configurations;

public class ShiftAttendanceConfiguration : IEntityTypeConfiguration<ShiftAttendance>
{
    public void Configure(EntityTypeBuilder<ShiftAttendance> builder)
    {
        builder.ToTable("ShiftAttendances");
        
        builder.HasKey(sa => sa.Id);
        
        builder.Property(sa => sa.EventType)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(sa => sa.Notes)
            .HasMaxLength(500);
            
        builder.Property(sa => sa.Location)
            .HasMaxLength(200);
            
        builder.Property(sa => sa.Device)
            .HasMaxLength(200);
            
        builder.HasOne<Shift>()
            .WithMany(s => s.Attendances)
            .HasForeignKey(sa => sa.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
