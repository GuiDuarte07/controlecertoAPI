﻿using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.Property(i => i.TotalAmount).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(i => i.TotalPaid).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(i => i.IsPaid).HasColumnType("boolean").IsRequired();
            //builder.Property(i => i.ClosingDate).HasColumnType("datetime");
            //builder.Property(i => i.DueDate).HasColumnType("datetime");
            //builder.Property(i => i.CreatedAt).HasColumnType("datetime");
            //builder.Property(i => i.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(i => i.CreditCard)
                .WithMany(cc => cc.Invoices)
                .HasForeignKey(i => i.CreditCardId);
            builder.HasMany(i => i.Transactions)
                .WithOne(ce => ce.Invoice)
                .HasForeignKey(ce => ce.InvoiceId);
            builder.HasMany(i => i.InvoicePayments)
                .WithOne(ip => ip.Invoice)
                .HasForeignKey(ip => ip.InvoiceId);
        }
    }
}
