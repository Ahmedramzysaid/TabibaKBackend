using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.AppointmentID);

        builder.Property(a => a.AppointmentID)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(a => a.AppointmentDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(a => a.AppointmentTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(a => a.AppointmentStatus)
            .IsRequired();

        builder.Property(a => a.AdditionalNotes)
            .HasMaxLength(2000);

        builder.Property(a => a.PatientID)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.DoctorID)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.MedicalRecord)
            .WithOne(m => m.Appointment)
            .HasForeignKey<Appointment>(a => a.MedicalRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Payment)
            .WithOne(p => p.Appointment)
            .HasForeignKey<Appointment>(a => a.PaymentID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.DoctorID, a.AppointmentDate, a.AppointmentTime })
            .HasDatabaseName("IX_Appointment_Doctor_DateTime");

        builder.HasIndex(a => a.PatientID)
            .HasDatabaseName("IX_Appointment_Patient");

        builder.HasIndex(a => a.AppointmentStatus)
            .HasDatabaseName("IX_Appointment_Status");
    }
}
