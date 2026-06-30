using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DigitalPrescriptionConfiguration : IEntityTypeConfiguration<DigitalPrescription>
{
    public void Configure(EntityTypeBuilder<DigitalPrescription> builder)
    {
        builder.HasKey(d => d.DigitalPrescriptionId);
        builder.Property(d => d.DigitalPrescriptionId).ValueGeneratedOnAdd();
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.HasOne(d => d.MedicalRecord)
            .WithMany(m => m.DigitalPrescriptions)
            .HasForeignKey(d => d.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
