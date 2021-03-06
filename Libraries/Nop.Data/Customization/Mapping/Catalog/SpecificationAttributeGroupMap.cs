using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public class SpecificationAttributeGroupMap : NopEntityTypeConfiguration<SpecificationAttributeGroup>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SpecificationAttributeGroup> builder)
        {
            builder.ToTable(nameof(SpecificationAttributeGroup));
            builder.HasKey(attribute => attribute.Id);

            builder.Property(attribute => attribute.Name).IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}
