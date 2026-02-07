using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ControleCerto.Models.Entities;

namespace ControleCerto.Models.MapConfig
{
    public class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
    {
        public void Configure(EntityTypeBuilder<Investment> builder)
        {
            builder.Property(i => i.Name).HasMaxLength(120).IsRequired();
            builder.Property(i => i.Description).HasMaxLength(300).IsRequired(false);
            builder.Property(i => i.StartDate).IsRequired();
            builder.Property(i => i.CurrentValue).HasColumnType("decimal(14,2)").IsRequired();

            builder.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Histories)
                .WithOne(h => h.Investment)
                .HasForeignKey(h => h.InvestmentId);
        }
    }
}
