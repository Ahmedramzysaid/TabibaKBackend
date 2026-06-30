using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DoctorAdviceVideoConfiguration : IEntityTypeConfiguration<DoctorAdviceVideo>
{
    public void Configure(EntityTypeBuilder<DoctorAdviceVideo> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.DoctorId)
            .HasMaxLength(450)
            .IsRequired();
        builder.HasOne(v => v.Doctor)
            .WithMany(d => d.DoctorAdviceVideos)
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(v => v.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(v => v.Description)
            .HasMaxLength(2000);
        builder.Property(v => v.VideoUrl)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(v => v.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(v => v.IsPublished)
            .HasDefaultValue(false);
    }
}
