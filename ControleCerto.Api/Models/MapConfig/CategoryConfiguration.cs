using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(c => c.Name).HasMaxLength(60).IsRequired();
            builder.Property(c => c.Icon).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Color).HasMaxLength(10).IsRequired();
            //builder.Property(c => c.CreatedAt).HasColumnType("datetime");
            //builder.Property(c => c.UpdatedAt).HasColumnType("datetime");

            builder.HasMany(c => c.Transactions)
                .WithOne(i => i.Category)
                .HasForeignKey(i => i.CategoryId);
            builder.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId);
        }
    }
}
