﻿using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Models.MapConfig
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(c => c.Name).HasMaxLength(45);
            builder.Property(c => c.Icon).HasMaxLength(45);
            builder.Property(c => c.BillType).HasMaxLength(45);
            builder.Property(c => c.CreatedAt).HasColumnType("datetime");
            builder.Property(c => c.UpdatedAt).HasColumnType("datetime");

            builder.HasMany(c => c.Incomes)
                .WithOne(i => i.Category)
                .HasForeignKey(i => i.CategoryId);
            builder.HasMany(c => c.Expenses)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId);
            builder.HasMany(c => c.CreditExpenses)
                .WithOne(ce => ce.Category)
                .HasForeignKey(ce => ce.CategoryId);
        }
    }
}
