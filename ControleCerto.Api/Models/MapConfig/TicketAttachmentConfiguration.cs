using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
    {
        public void Configure(EntityTypeBuilder<TicketAttachment> builder)
        {
            builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            builder.Property(a => a.ContentType).IsRequired().HasMaxLength(120);
            builder.Property(a => a.StorageKey).IsRequired().HasMaxLength(500);
            builder.Property(a => a.Url).IsRequired().HasMaxLength(800);
            builder.HasIndex(a => new { a.TicketId, a.CreatedAt });
            builder.HasIndex(a => new { a.TicketMessageId, a.CreatedAt });

            builder.HasOne(a => a.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TicketId);

            builder.HasOne(a => a.TicketMessage)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.TicketMessageId)
                .IsRequired(false);

            builder.HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId);
        }
    }
}

