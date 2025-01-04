using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleCerto.Models.MapConfig
{
    public class CategoryLimitConfiguration : IEntityTypeConfiguration<CategoryLimit>
    {
        public void Configure(EntityTypeBuilder<CategoryLimit> builder)
        {
            builder.Property(c => c.Amount).HasColumnType("decimal(10,2)").IsRequired();

            builder.HasOne(cl => cl.Category)
                .WithMany(c => c.Limits)
                .HasForeignKey(cl => cl.CategoryId);
        }
    }
}
