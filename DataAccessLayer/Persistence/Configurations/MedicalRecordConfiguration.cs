using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.HasKey(m => m.MedicalRecordId);

        builder.Property(m => m.MedicalRecordId)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.PatientId)
            .HasMaxLength(450) // Same as AspNetUsers.Id
            .IsRequired();
        
        builder.HasOne<Patient>(m => m.Patient)
            .WithOne(p => p.MedicalRecord)
            .HasForeignKey<MedicalRecord>(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cascade path issues

        builder.HasMany<Prescription>(m => m.Prescriptions)
            .WithOne(p => p.MedicalRecord)
            .HasForeignKey(p => p.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany<Testing>(m => m.Testings)
            .WithOne(t => t.MedicalRecord)
            .HasForeignKey(t => t.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
