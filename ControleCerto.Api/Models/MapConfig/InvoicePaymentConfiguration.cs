using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
    {
        public void Configure(EntityTypeBuilder<InvoicePayment> builder)
        {
            builder.Property(ip => ip.AmountPaid).HasColumnType("decimal(10,2)");
            builder.Property(ip => ip.Description).HasMaxLength(100);
            //builder.Property(ip => ip.PaymentDate).HasColumnType("datetime");
            // builder.Property(ip => ip.CreatedAt).HasColumnType("datetime");

            builder.HasOne(ip => ip.Invoice)
                .WithMany(i => i.InvoicePayments)
                .HasForeignKey(ip => ip.InvoiceId);
            builder.HasOne(ip => ip.Account)
                .WithMany(a => a.InvoicePayments)
                .HasForeignKey(i => i.AccountId);
        }
    }
}
