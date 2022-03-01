using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Discounts
{
    public partial class DiscountCouponCodes : BaseEntity
    {
        public int DiscountId { get; set; }
        public string CouponCode { get; set; }
        public int IsUsedCount { get; set; }
    }
}
