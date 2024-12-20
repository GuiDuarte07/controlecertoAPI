using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(n => n.Title).IsRequired().HasMaxLength(100).IsRequired();
            builder.Property(n => n.Message).IsRequired().HasMaxLength(600).IsRequired();
            builder.Property(n => n.ActionPath).HasMaxLength(100).IsRequired(false);
            builder.Property(n => n.IsRead).HasColumnType("boolean").IsRequired();

            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);
        }

    }

}
