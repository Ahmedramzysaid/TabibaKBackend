using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> modelBuilder)
        {
            modelBuilder.HasKey(p => p.Id);
            
            modelBuilder.Property(p => p.Id)
                .HasMaxLength(450)
                .IsRequired();
            
            modelBuilder.HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}
