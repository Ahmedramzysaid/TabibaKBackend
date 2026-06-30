using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class ChatAuditLogConfiguration : IEntityTypeConfiguration<ChatAuditLog>
{
    public void Configure(EntityTypeBuilder<ChatAuditLog> builder)
    {
        builder.ToTable("ChatAuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.ConversationId).IsRequired();
        builder.Property(a => a.ActorId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(32).IsRequired();
        builder.Property(a => a.Metadata).HasMaxLength(4000);
        builder.Property(a => a.CreatedAtUtc).HasColumnType("datetime2(2)").IsRequired();

        builder.HasIndex(a => new { a.ConversationId, a.CreatedAtUtc });
        builder.HasIndex(a => new { a.ActorId, a.CreatedAtUtc });
    }
}
