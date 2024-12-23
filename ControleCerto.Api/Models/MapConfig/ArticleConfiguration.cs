using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.MapConfig
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
            builder.Property(a => a.MdFileName).HasMaxLength(400).IsRequired();

            builder.HasIndex(a => a.Title).IsUnique();
            builder.HasIndex(a => a.MdFileName).IsUnique();

            builder.HasOne(a => a.User)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.UserId);
        }
        
    }
}