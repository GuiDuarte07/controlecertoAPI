using Finantech.Models.Entities;
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
            builder.Property(u => u.EmailConfirmed).HasColumnType("boolean");
            //builder.Property(u => u.CreatedAt).HasColumnType("datetime");
            //builder.Property(u => u.UpdatedAt).HasColumnType("datetime");

            builder.HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);
            builder.HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
        }
    }
}
