using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public class PackagesMap : NopEntityTypeConfiguration<Packages>
    {
        public override void Configure(EntityTypeBuilder<Packages> builder)
        {
            builder.ToTable(nameof(Packages));

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(400).IsRequired();

            base.Configure(builder);
        }
    }
}
