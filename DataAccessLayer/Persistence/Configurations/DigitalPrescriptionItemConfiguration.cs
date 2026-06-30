using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DigitalPrescriptionItemConfiguration : IEntityTypeConfiguration<DigitalPrescriptionItem>
{
    public void Configure(EntityTypeBuilder<DigitalPrescriptionItem> builder)
    {
        builder.HasKey(d => d.DigitalPrescriptionItemId);
        builder.Property(d => d.DigitalPrescriptionItemId).ValueGeneratedOnAdd();
        builder.Property(d => d.MedicineName).HasMaxLength(500);
        builder.Property(d => d.Spotlights).HasMaxLength(2000);

        builder.HasOne(d => d.DigitalPrescription)
            .WithMany(dp => dp.Items)
            .HasForeignKey(d => d.DigitalPrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
