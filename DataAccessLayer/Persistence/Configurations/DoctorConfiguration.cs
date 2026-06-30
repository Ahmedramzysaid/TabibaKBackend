using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .HasMaxLength(450)
            .IsRequired();
        
        builder.HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(d => d.Specialization)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(d => d.IdNo)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(d => d.IdImageFrontUrl)
            .HasMaxLength(500);
        
        builder.Property(d => d.IdImageBackUrl)
            .HasMaxLength(500);

        builder.Property(d => d.Price)
            .HasPrecision(18, 2);

        builder.Property(d => d.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.Rating).HasPrecision(3, 2);
        builder.Property(d => d.RatingCount).HasDefaultValue(0);
    }
}
