using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DoctorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.SlotDurationMinutes)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(s => new { s.DoctorId, s.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("IX_DoctorSchedule_Doctor_Day");

        builder.HasOne(s => s.Doctor)
            .WithMany(d => d.Schedules)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
