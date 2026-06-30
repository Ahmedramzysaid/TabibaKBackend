using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DoctorRatingConfiguration : IEntityTypeConfiguration<DoctorRating>
{
    public void Configure(EntityTypeBuilder<DoctorRating> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.AppointmentID).IsRequired();
        builder.HasOne(r => r.Appointment)
            .WithMany()
            .HasForeignKey(r => r.AppointmentID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.DoctorID).HasMaxLength(450).IsRequired();
        builder.HasOne(r => r.Doctor)
            .WithMany()
            .HasForeignKey(r => r.DoctorID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.PatientID).HasMaxLength(450).IsRequired();
        builder.HasOne(r => r.Patient)
            .WithMany()
            .HasForeignKey(r => r.PatientID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(r => new { r.AppointmentID }).IsUnique(); // One rating per appointment
    }
}
