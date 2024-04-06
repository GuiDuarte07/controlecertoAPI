﻿using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finantech.Models.MapConfig
{
    public class CreditPurchaseConfiguration : IEntityTypeConfiguration<CreditPurchase>
    {
        public void Configure(EntityTypeBuilder<CreditPurchase> builder)
        {
            builder.Property(cp => cp.TotalAmount).HasColumnType("decimal(10,2)");
            builder.Property(cp => cp.PurchaseDate).HasColumnType("datetime");
            builder.Property(cp => cp.Paid).HasColumnType("tinyint");
            builder.Property(cp => cp.Destination).HasMaxLength(45);
            builder.Property(cp => cp.Description).HasMaxLength(100);
            builder.Property(cp => cp.CreatedAt).HasColumnType("datetime");
            builder.Property(cp => cp.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(cp => cp.CreditCard)
                .WithMany(cc => cc.CreditPurchases)
                .HasForeignKey(cp => cp.CreditCardId);

            builder.HasMany(cp => cp.CreditExpenses)
                .WithOne(ce => ce.CreditPurchase)
                .HasForeignKey(ce => ce.CreditPurchaseId);
        }
    }
}
