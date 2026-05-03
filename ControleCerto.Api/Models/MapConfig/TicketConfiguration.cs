using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.Property(t => t.Subject).IsRequired().HasMaxLength(140);
            builder.Property(t => t.Status).IsRequired();
            builder.Property(t => t.Priority).IsRequired();
            builder.HasIndex(t => new { t.UserId, t.UpdatedAt });
            builder.HasIndex(t => new { t.Status, t.UpdatedAt });

            builder.HasQueryFilter(t => !t.User!.Deleted);

            builder.HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId);

            builder.HasMany(t => t.Messages)
                .WithOne(m => m.Ticket)
                .HasForeignKey(m => m.TicketId);

            builder.HasMany(t => t.Attachments)
                .WithOne(a => a.Ticket)
                .HasForeignKey(a => a.TicketId);
        }
    }
}

