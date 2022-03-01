using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Discounts
{
    public partial class DiscountCouponCodesModel : BaseNopEntityModel
    {
        public int DiscountId { get; set; }
        public string CouponCode { get; set; }
        [NopResourceDisplayName("Admin.DiscountCouponCodes.Fields.NoOfCodes")]
        public int NoOfCodes { get; set; }
        [NopResourceDisplayName("Admin.DiscountCouponCodes.Fields.CodeLength")]
        public int CodeLength { get; set; }
        [NopResourceDisplayName("Admin.DiscountCouponCodes.Fields.Prefix")]
        public string Prefix { get; set; }
        [NopResourceDisplayName("Admin.DiscountCouponCodes.Fields.Suffix")]
        public string Suffix { get; set; }
        [NopResourceDisplayName("Admin.DiscountCouponCodes.Fields.CharacterSet")]
        public string CharacterSet { get; set; }
    }
}
