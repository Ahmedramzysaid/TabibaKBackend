using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class AdviceCommentConfiguration : IEntityTypeConfiguration<AdviceComment>
{
    public void Configure(EntityTypeBuilder<AdviceComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.AdvicePostId).IsRequired();
        builder.HasOne(c => c.AdvicePost)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.AdvicePostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.UserId).HasMaxLength(450).IsRequired();
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Content)
            .HasMaxLength(2000)
            .IsRequired();
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);
    }
}
