using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
    {
        public void Configure(EntityTypeBuilder<TicketMessage> builder)
        {
            builder.Property(m => m.Body).IsRequired();
            builder.Property(m => m.AuthorRole).IsRequired();
            builder.HasIndex(m => new { m.TicketId, m.CreatedAt });

            builder.HasOne(m => m.Ticket)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TicketId);

            builder.HasOne(m => m.AuthorUser)
                .WithMany()
                .HasForeignKey(m => m.AuthorUserId);

            builder.HasMany(m => m.Attachments)
                .WithOne(a => a.TicketMessage)
                .HasForeignKey(a => a.TicketMessageId);
        }
    }
}

