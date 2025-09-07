using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class RecurrenceRuleConfiguration : IEntityTypeConfiguration<RecurrenceRule>
    {
        public void Configure(EntityTypeBuilder<RecurrenceRule> builder)
        {
            builder.ToTable("RecurrenceRules");

            builder.HasKey(rr => rr.Id);

            builder.Property(rr => rr.Id)
                .ValueGeneratedOnAdd();

            builder.Property(rr => rr.Frequency)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(rr => rr.IsEveryDay)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(rr => rr.DaysOfWeek)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(rr => rr.DayOfWeek)
                .IsRequired(false);

            builder.Property(rr => rr.DayOfMonth)
                .IsRequired(false);

            builder.Property(rr => rr.MonthOfYear)
                .IsRequired(false);

            builder.Property(rr => rr.DayOfMonthForYearly)
                .IsRequired(false);

            builder.Property(rr => rr.Interval)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(rr => rr.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Relacionamentos
            builder.HasMany(rr => rr.RecurringTransactions)
                .WithOne(rt => rt.RecurrenceRule)
                .HasForeignKey(rt => rt.RecurrenceRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.HasIndex(rr => rr.Frequency);
            builder.HasIndex(rr => rr.DayOfWeek);
            builder.HasIndex(rr => rr.DayOfMonth);
            builder.HasIndex(rr => rr.MonthOfYear);
        }
    }
}
