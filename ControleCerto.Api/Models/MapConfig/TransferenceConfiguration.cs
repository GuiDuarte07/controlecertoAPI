using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class TransferenceConfiguration : IEntityTypeConfiguration<Transference>
    {
        public void Configure(EntityTypeBuilder<Transference> builder)
        {
            builder.Property(t => t.Description).HasMaxLength(100).IsRequired(false);
            builder.Property(t => t.Amount).HasColumnType("decimal(10,2)");
            builder.Property(t => t.PurchaseDate).HasMaxLength(45);
            //builder.Property(t => t.CreatedAt).HasColumnType("datetime");
            //builder.Property(t => t.UpdatedAt).HasColumnType("datetime");

            builder.HasKey(t => t.Id);


            builder.HasOne(t => t.AccountDestiny)
                .WithMany()
                .HasForeignKey(t => t.AccountDestinyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AccountOrigin)
                .WithMany()
                .HasForeignKey(t => t.AccountOriginId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
