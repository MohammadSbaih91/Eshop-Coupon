using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Discounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Discounts
{
    public partial class DiscountCouponCodesMap : NopEntityTypeConfiguration<DiscountCouponCodes>
    {
        #region Methods
        public override void Configure(EntityTypeBuilder<DiscountCouponCodes> builder)
        {
            builder.ToTable(nameof(DiscountCouponCodes));
            builder.HasKey(discountCodes => discountCodes.Id);

            builder.Property(discountCodes => discountCodes.CouponCode).HasMaxLength(100);
        }
        #endregion
    }
}
