using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Models.MapConfig
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Balance).HasColumnType("decimal(10,2)");
            builder.Property(a => a.Description).HasMaxLength(100);
            builder.Property(a => a.Bank).HasMaxLength(45);
            builder.Property(a => a.Color).HasMaxLength(10);
            builder.Property(ce => ce.CreatedAt).HasColumnType("datetime");
            builder.Property(ce => ce.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId);
            builder.HasOne(a => a.CreditCard)
                .WithOne(cc => cc.Account)
                .HasForeignKey<CreditCard>(a => a.AccountId);
            builder.HasMany(a => a.Incomes)
                .WithOne(i => i.Account)
                .HasForeignKey(i => i.AccountId);
            builder.HasMany(a => a.Expenses)
                .WithOne(e => e.Account)
                .HasForeignKey(e => e.AccountId);
            builder.HasMany(a => a.CreditExpenses)
                .WithOne(ce => ce.Account)
                .HasForeignKey(c => c.AccountId);
        }
    }
}
