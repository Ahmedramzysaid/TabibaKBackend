using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class TestingConfiguration : IEntityTypeConfiguration<Testing>
{
    public void Configure(EntityTypeBuilder<Testing> builder)
    {
        builder.HasKey(t => t.TestingId);

        builder.Property(t => t.TestingId)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.DateExam)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(t => t.FilePath)
            .HasMaxLength(500);

        builder.Property(t => t.FileType)
            .HasMaxLength(50);

    }
}
