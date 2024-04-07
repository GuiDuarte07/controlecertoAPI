using Finantech.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Models.MapConfig
{
    public class CategoryDefaultConfiguration : IEntityTypeConfiguration<CategoryDefault>
    {
        public void Configure(EntityTypeBuilder<CategoryDefault> builder)
        {
            builder.Property(cd => cd.Name).HasMaxLength(45);
            builder.Property(c => c.Color).HasMaxLength(10);
            builder.Property(cd => cd.Icon).HasMaxLength(45);
        }
    }
}
