using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
    {
        public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
        {
            builder.ToTable("RecurringTransactions");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .ValueGeneratedOnAdd();

            builder.Property(rt => rt.Description)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(rt => rt.Destination)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(rt => rt.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");


            builder.Property(rt => rt.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(rt => rt.StartDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(rt => rt.EndDate)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(rt => rt.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(rt => rt.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(rt => rt.UpdatedAt)
                .IsRequired(false);

            builder.Property(t => t.JustForRecord).HasDefaultValue(false);

            // Global query filter para excluir transações recorrentes de usuários deletados
            builder.HasQueryFilter(rt => !rt.User!.Deleted);

            // Relacionamentos
            builder.HasOne(rt => rt.Account)
                .WithMany()
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rt => rt.Category)
                .WithMany()
                .HasForeignKey(rt => rt.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rt => rt.RecurrenceRule)
                .WithMany(rr => rr.RecurringTransactions)
                .HasForeignKey(rt => rt.RecurrenceRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(rt => rt.Instances)
                .WithOne(rti => rti.RecurringTransaction)
                .HasForeignKey(rti => rti.RecurringTransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.IsActive);
            builder.HasIndex(rt => rt.StartDate);
            builder.HasIndex(rt => rt.EndDate);
            builder.HasIndex(rt => rt.Type);
            builder.HasIndex(rt => new { rt.UserId, rt.IsActive, rt.StartDate });
        }
    }
}
