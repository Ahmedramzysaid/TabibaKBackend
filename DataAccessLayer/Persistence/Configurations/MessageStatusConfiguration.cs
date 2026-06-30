using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class MessageStatusConfiguration : IEntityTypeConfiguration<MessageStatus>
{
    public void Configure(EntityTypeBuilder<MessageStatus> builder)
    {
        builder.HasKey(ms => new { ms.MessageId, ms.RecipientId });

        builder.Property(ms => ms.RecipientId).HasMaxLength(450).IsRequired();
        builder.Property(ms => ms.Status).IsRequired();
        builder.Property(ms => ms.UpdatedAtUtc).HasColumnType("datetime2(2)").IsRequired();

        builder.HasOne(ms => ms.Message)
            .WithMany(m => m.MessageStatuses)
            .HasForeignKey(ms => ms.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ms => ms.Recipient)
            .WithMany()
            .HasForeignKey(ms => ms.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ms => new { ms.RecipientId, ms.UpdatedAtUtc });
    }
}
