using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Finantech.Models.Entities;

namespace Finantech.Models.MapConfig
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(t => t.Description).HasMaxLength(100).IsRequired();
            builder.Property(t => t.Amount).HasColumnType("decimal(10,2)").IsRequired();
            //builder.Property(e => e.PurchaseDate).HasColumnType("datetime");
            builder.Property(t => t.Destination).HasMaxLength(80);
            builder.Property(t => t.JustForRecord).HasDefaultValue(false);
            builder.Property(t => t.Observations).HasMaxLength(300);
            //builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            //builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(e => e.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(e => e.AccountId);

            builder.HasOne(e => e.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
