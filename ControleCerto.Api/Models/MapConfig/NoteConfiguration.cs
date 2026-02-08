using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            builder.Property(n => n.Content)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(n => n.Year)
                .HasColumnType("integer");

            builder.Property(n => n.Month)
                .HasColumnType("integer");

            builder.Property(n => n.UserId)
                .HasColumnType("integer")
                .IsRequired();

            builder.Property(n => n.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            builder.Property(n => n.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => new { n.UserId, n.Year, n.Month });

            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.ToTable("Notes");
        }
    }
}
