using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finantech.Models.MapConfig
{
    public class IncomeConfiguration : IEntityTypeConfiguration<Income>
    {
        public void Configure(EntityTypeBuilder<Income> builder)
        {
            builder.Property(i => i.Description).HasMaxLength(100);
            builder.Property(i => i.Amount).HasColumnType("decimal(10,2)");
            builder.Property(i => i.IncomeType).HasMaxLength(45);
            builder.Property(i => i.PurchaseDate).HasColumnType("datetime");
            builder.Property(i => i.Origin).HasMaxLength(45);
            builder.Property(i => i.CreatedAt).HasColumnType("datetime");
            builder.Property(i => i.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(i => i.Account)
                .WithMany(a => a.Incomes)
                .HasForeignKey(i => i.AccountId);

            builder.HasOne(i => i.Category)
                .WithMany(c => c.Incomes)
                .HasForeignKey(i => i.CategoryId);
        }
    }
}
