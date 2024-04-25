using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finantech.Models.MapConfig
{
    public class CreditExpenseConfiguration : IEntityTypeConfiguration<CreditExpense>
    {
        public void Configure(EntityTypeBuilder<CreditExpense> builder)
        {
            builder.Property(ce => ce.Amount).HasColumnType("decimal(10,2)");
            builder.Property(ce => ce.Description).HasMaxLength(100);
            //builder.Property(ce => ce.PurchaseDate).HasColumnType("datetime");
            builder.Property(ce => ce.InstallmentNumber).HasColumnType("int");
            builder.Property(ce => ce.Destination).HasMaxLength(45);
            //builder.Property(ce => ce.CreatedAt).HasColumnType("datetime");
            //builder.Property(ce => ce.UpdatedAt).HasColumnType("datetime");

            builder.HasKey(ce => ce.Id );

            builder.HasOne(ce => ce.CreditPurchase)
                .WithMany(cp => cp.CreditExpenses)
                .HasForeignKey(ce => ce.CreditPurchaseId);

            builder.HasOne(ce => ce.Account)
                .WithMany(a => a.CreditExpenses)
                .HasForeignKey(ce => ce.AccountId);

            builder.HasOne(ce => ce.Invoice)
                .WithMany(i => i.CreditExpenses)
                .HasForeignKey(ce => ce.InvoiceId);

            builder.HasOne(ce => ce.Category)
                .WithMany(c => c.CreditExpenses)
                .HasForeignKey(ce => ce.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
