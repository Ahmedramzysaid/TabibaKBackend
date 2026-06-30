using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DoctorAdvicePostConfiguration : IEntityTypeConfiguration<DoctorAdvicePost>
{
    public void Configure(EntityTypeBuilder<DoctorAdvicePost> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DoctorId)
            .HasMaxLength(450)
            .IsRequired();
        builder.HasOne(p => p.Doctor)
            .WithMany(d => d.AdvicePosts)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Content)
            .HasMaxLength(4000)
            .IsRequired();
        builder.Property(p => p.ImageUrl)
            .HasMaxLength(1000);
        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(p => p.IsPublished)
            .HasDefaultValue(false);
    }
}
