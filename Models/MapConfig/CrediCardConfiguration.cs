using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Models.MapConfig
{
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.Property(cc => cc.Number).HasMaxLength(15).HasColumnType("char(15)");
            builder.Property(cc => cc.Limit).HasColumnType("decimal(10,2)");
            builder.Property(cc => cc.Description).HasMaxLength(100);
            builder.Property(cc => cc.CardBrand).HasMaxLength(45);
            builder.Property(cc => cc.CreditType).HasMaxLength(45);
            builder.Property(cc => cc.CreatedAt).HasColumnType("date");
            builder.Property(cc => cc.UpdatedAt).HasColumnType("date");

            builder.HasKey(cc => cc.Number);
            builder.HasIndex(u => u.Number).IsUnique();

            builder.HasOne(cc => cc.Account)
                .WithOne(a => a.CreditCard)
                .HasForeignKey<CreditCard>(cc => cc.AccountId);
            builder.HasMany(cc => cc.Invoices)
                .WithOne(i => i.CreditCard)
                .HasForeignKey(i => i.CreditCardId);
            builder.HasMany(cc => cc.CreditPurchases)
                .WithOne(cp => cp.CreditCard)
                .HasForeignKey(cp => cp.CreditCardId);
        }
    }
}
