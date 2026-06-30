using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class AdviceLikeConfiguration : IEntityTypeConfiguration<AdviceLike>
{
    public void Configure(EntityTypeBuilder<AdviceLike> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.Property(l => l.AdvicePostId).IsRequired();
        builder.HasOne(l => l.AdvicePost)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.AdvicePostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(l => l.UserId).HasMaxLength(450).IsRequired();
        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(l => new { l.AdvicePostId, l.UserId }).IsUnique();
    }
}
