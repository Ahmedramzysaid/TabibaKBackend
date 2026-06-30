using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations
{
    public class SavedPostConfiguration : IEntityTypeConfiguration<SavedPost>
    {
        public void Configure(EntityTypeBuilder<SavedPost> builder)
        {
            builder.HasKey(sp => sp.Id);

            builder.HasIndex(sp => new { sp.UserId, sp.DoctorAdvicePostId }).IsUnique();

            builder.HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sp => sp.DoctorAdvicePost)
                .WithMany()
                .HasForeignKey(sp => sp.DoctorAdvicePostId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
