using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ControleCerto.Models.Entities;

namespace ControleCerto.Models.MapConfig
{
    public class InvestmentHistoryConfiguration : IEntityTypeConfiguration<InvestmentHistory>
    {
        public void Configure(EntityTypeBuilder<InvestmentHistory> builder)
        {
            builder.Property(h => h.ChangeAmount).HasColumnType("decimal(14,2)").IsRequired();
            builder.Property(h => h.TotalValue).HasColumnType("decimal(14,2)").IsRequired();
            builder.Property(h => h.Note).HasMaxLength(400).IsRequired(false);
            builder.Property(h => h.OccurredAt).IsRequired();

            builder.HasOne(h => h.Investment)
                .WithMany(i => i.Histories)
                .HasForeignKey(h => h.InvestmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.SourceAccount)
                .WithMany()
                .HasForeignKey(h => h.SourceAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
