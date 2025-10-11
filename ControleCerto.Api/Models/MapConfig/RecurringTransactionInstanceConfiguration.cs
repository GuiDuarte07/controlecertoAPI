using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class RecurringTransactionInstanceConfiguration : IEntityTypeConfiguration<RecurringTransactionInstance>
    {
        public void Configure(EntityTypeBuilder<RecurringTransactionInstance> builder)
        {
            builder.ToTable("RecurringTransactionInstances");

            builder.HasKey(rti => rti.Id);

            builder.Property(rti => rti.Id)
                .ValueGeneratedOnAdd();

            builder.Property(rti => rti.RecurringTransactionId)
                .IsRequired();

            builder.Property(rti => rti.ScheduledDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(rti => rti.ProcessedDate)
                .HasColumnType("timestamp")
                .IsRequired(false);

            builder.Property(rti => rti.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(rti => rti.ActualTransactionId)
                .IsRequired(false);

            builder.Property(rti => rti.RejectionReason)
                .HasMaxLength(300)
                .IsRequired(false);

            builder.Property(rti => rti.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(rti => rti.UpdatedAt)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(rti => rti.RecurringTransaction)
                .WithMany(rt => rt.Instances)
                .HasForeignKey(rti => rti.RecurringTransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rti => rti.ActualTransaction)
                .WithMany()
                .HasForeignKey(rti => rti.ActualTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Índices para performance
            builder.HasIndex(rti => rti.RecurringTransactionId);
            builder.HasIndex(rti => rti.ScheduledDate);
            builder.HasIndex(rti => rti.Status);
            builder.HasIndex(rti => rti.ProcessedDate);
            builder.HasIndex(rti => new { rti.RecurringTransactionId, rti.ScheduledDate });
            builder.HasIndex(rti => new { rti.Status, rti.ScheduledDate });
        }
    }
}
