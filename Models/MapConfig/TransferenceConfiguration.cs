using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Models.MapConfig
{
    public class TransferenceConfiguration : IEntityTypeConfiguration<Transference>
    {
        public void Configure(EntityTypeBuilder<Transference> builder)
        {
            builder.Property(t => t.Description).HasMaxLength(100);
            builder.Property(t => t.Amount).HasColumnType("decimal(10,2)");
            builder.Property(t => t.PurchaseDate).HasMaxLength(45);
            builder.Property(t => t.CreatedAt).HasMaxLength(45);
            builder.Property(t => t.UpdatedAt).HasMaxLength(45);

            builder.HasKey(t => new { t.Id, t.AccountDestinyId, t.AccountOriginId });

            builder.HasOne(t => t.AccountDestiny)
                .WithMany()
                .HasForeignKey(t => t.AccountDestinyId);

            builder.HasOne(t => t.AccountOrigin)
                .WithMany()
                .HasForeignKey(t => t.AccountOriginId);
        }
    }
}
