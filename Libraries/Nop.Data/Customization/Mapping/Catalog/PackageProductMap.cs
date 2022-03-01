using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public class PackageProductMap : NopEntityTypeConfiguration<PackageProduct>
    {
        public override void Configure(EntityTypeBuilder<PackageProduct> builder)
        {
            builder.ToTable(nameof(PackageProduct));

            builder.HasKey(p => p.Id);

            base.Configure(builder);
        }
    }
}
