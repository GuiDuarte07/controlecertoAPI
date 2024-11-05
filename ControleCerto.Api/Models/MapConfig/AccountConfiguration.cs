using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Balance).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(a => a.Description).HasMaxLength(100).IsRequired(false);
            builder.Property(a => a.Bank).HasMaxLength(45).IsRequired();
            builder.Property(a => a.Color).HasMaxLength(10).IsRequired();
            builder.Property(a => a.Deleted).HasDefaultValue(false);
            //builder.Property(ce => ce.CreatedAt).HasColumnType("datetime");
            //builder.Property(ce => ce.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId);
            builder.HasOne(a => a.CreditCard)
                .WithOne(cc => cc.Account)
                .HasForeignKey<CreditCard>(a => a.AccountId);
            builder.HasMany(a => a.Transactions)
                .WithOne(i => i.Account)
                .HasForeignKey(i => i.AccountId);
        }
    }
}
