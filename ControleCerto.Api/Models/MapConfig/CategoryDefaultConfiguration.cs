﻿using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class CategoryDefaultConfiguration : IEntityTypeConfiguration<CategoryDefault>
    {
        public void Configure(EntityTypeBuilder<CategoryDefault> builder)
        {
            builder.Property(cd => cd.Name).HasMaxLength(60).IsRequired();
            builder.Property(c => c.Color).HasMaxLength(10).IsRequired();
            builder.Property(cd => cd.Icon).HasMaxLength(45).IsRequired();
        }
    }
}
