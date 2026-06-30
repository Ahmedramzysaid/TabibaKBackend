using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");

        builder.Property(c => c.PatientId).HasMaxLength(450).IsRequired();
        builder.Property(c => c.DoctorId).HasMaxLength(450).IsRequired();
        builder.Property(c => c.CreatedAtUtc).HasColumnType("datetime2(2)").IsRequired();
        builder.Property(c => c.UpdatedAtUtc).HasColumnType("datetime2(2)").IsRequired();

        builder.HasOne(c => c.Patient)
            .WithMany()
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Doctor)
            .WithMany()
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.PatientId, c.DoctorId }).IsUnique();
        builder.HasIndex(c => c.PatientId);
        builder.HasIndex(c => c.DoctorId);
    }
}
