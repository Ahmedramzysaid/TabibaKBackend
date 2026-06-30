using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.SenderId).HasMaxLength(450).IsRequired();
        builder.Property(m => m.Content).HasMaxLength(8000).IsRequired();
        builder.Property(m => m.CreatedAtUtc).HasColumnType("datetime2(2)").IsRequired();
        builder.Property(m => m.EditedAtUtc).HasColumnType("datetime2(2)");
        builder.Property(m => m.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ConversationId, m.CreatedAtUtc });
    }
}
