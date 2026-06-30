using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.HasKey(p => p.PrescriptionId);

        builder.Property(p => p.PrescriptionId)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.FilePath)
            .HasMaxLength(500); // Path to file

        builder.Property(p => p.FileType)
            .HasMaxLength(100); // File type (application/pdf, image/png, etc.)

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int in database

    }
}
