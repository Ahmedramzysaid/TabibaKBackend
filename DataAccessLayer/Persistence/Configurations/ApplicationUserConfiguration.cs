using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.DateOfRegistration)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(u => u.DateOfBirth)
            .IsRequired();

        builder.Property(u => u.Gender)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("IX_AspNetUsers_NormalizedEmail_Unique")
            .HasFilter("[NormalizedEmail] IS NOT NULL");

        builder.Property(u => u.Latitude)
            .HasColumnType("float")
            .IsRequired();

        builder.Property(u => u.Longitude)
            .HasColumnType("float")
            .IsRequired();

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}
