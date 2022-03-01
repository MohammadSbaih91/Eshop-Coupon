using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Represents a product mapping configuration
    /// </summary>
    public partial class ProductMap
    {
        protected override void PostConfigure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(product => product.SplitAmount).HasColumnType("decimal(18, 4)");
            builder.Property(product => product.SplitAmount2).HasColumnType("decimal(18, 4)");
            builder.Property(product => product.IsStudentIdNeeded).HasColumnType("bit");
            builder.Ignore(order => order.IsTaxSplitEnabled);
//            builder.Ignore(order => order.PaymentType);

            base.PostConfigure(builder);
        }
    }
}