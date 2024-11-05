using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.Property(cc => cc.TotalLimit).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(cc => cc.UsedLimit).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(cc => cc.Description).HasMaxLength(100).IsRequired();
            builder.Property(cc => cc.SkipWeekend).HasDefaultValue(true).IsRequired();
            //builder.Property(cc => cc.CreatedAt).HasColumnType("datetime");
            //builder.Property(cc => cc.UpdatedAt).HasColumnType("datetime");

            builder.HasKey(cc => cc.Id);

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
