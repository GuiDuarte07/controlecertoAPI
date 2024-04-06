﻿using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finantech.Models.MapConfig
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(60);
            builder.Property(u => u.PasswordHash).HasMaxLength(60);
            builder.Property(u => u.EmailConfirmed).HasColumnType("tinyint");
            builder.Property(u => u.CreatedAt).HasColumnType("date");
            builder.Property(u => u.UpdatedAt).HasColumnType("date");

            builder.HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);
        }
    }
}