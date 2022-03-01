using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace Widgets.CustomerOrderReview.Data
{
    public partial class CustomerOrderReviewMap : NopEntityTypeConfiguration<Domain.CustomerOrderReview>
    {
        #region Methods

        public override void Configure(EntityTypeBuilder<Domain.CustomerOrderReview> builder)
        {
            builder.ToTable(nameof(Domain.CustomerOrderReview));
            builder.HasKey(review => review.Id);
            builder.Ignore(review => review.CustomerOrderReviewType);
        }

        #endregion
    }
}