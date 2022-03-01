using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Represents a product attribute value mapping configuration
    /// </summary>
    public partial class ProductAttributeValueMap
    {
        #region Methods

        protected override void PostConfigure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.Property(product => product.SplitAmount).HasColumnType("decimal(18, 4)");
            builder.Property(product => product.SplitAmount2).HasColumnType("decimal(18, 4)");
            base.PostConfigure(builder);
        }

        #endregion
    }
}