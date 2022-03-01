using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public class ProductSimcardMap : NopEntityTypeConfiguration<ProductSimcardMapping>
    {
        public override void Configure(EntityTypeBuilder<ProductSimcardMapping> builder)
        {
            builder.ToTable("Product_Simcard_Mapping");
            builder.HasKey(p => p.Id);

            base.Configure(builder);
        }
    }
}

