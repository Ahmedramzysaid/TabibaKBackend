using DomainLayer.Enums;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class CallConfiguration : IEntityTypeConfiguration<Call>
{
    public void Configure(EntityTypeBuilder<Call> builder)
    {
        builder.ToTable("Calls");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");

        builder.Property(c => c.ConversationId).IsRequired();
        builder.Property(c => c.CallerId).HasMaxLength(450).IsRequired();
        builder.Property(c => c.CalleeId).HasMaxLength(450);
        builder.Property(c => c.StartedAtUtc).HasColumnType("datetime2(2)").IsRequired();
        builder.Property(c => c.EndedAtUtc).HasColumnType("datetime2(2)");
        builder.Property(c => c.Status).IsRequired();

        builder.HasOne(c => c.Conversation)
            .WithMany()
            .HasForeignKey(c => c.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ConversationId, c.StartedAtUtc });
    }
}
