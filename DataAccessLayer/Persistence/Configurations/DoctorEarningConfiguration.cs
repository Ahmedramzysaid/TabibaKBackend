using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DoctorEarningConfiguration : IEntityTypeConfiguration<DoctorEarning>
{
    public void Configure(EntityTypeBuilder<DoctorEarning> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DoctorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(e => e.DailyEarnings)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.AppointmentCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => new { e.DoctorId, e.Date })
            .IsUnique()
            .HasDatabaseName("IX_DoctorEarning_Doctor_Date");

        builder.HasOne(e => e.Doctor)
            .WithMany(d => d.Earnings)
            .HasForeignKey(e => e.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
