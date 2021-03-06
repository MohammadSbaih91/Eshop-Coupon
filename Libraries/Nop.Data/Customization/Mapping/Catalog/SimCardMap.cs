using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public class SimCardMap : NopEntityTypeConfiguration<SimCard>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SimCard> builder)
        {
            builder.ToTable(nameof(SimCard));
            builder.HasKey(card => card.Id);

            builder.Property(card => card.CardNumber).IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}
