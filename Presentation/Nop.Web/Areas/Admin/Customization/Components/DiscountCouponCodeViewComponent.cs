using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Discounts;
using Nop.Web.Framework.Components;

namespace Nop.Web.Areas.Admin.Components
{
    public class DiscountCouponCodeViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(int discountId)
        {
            var model = new DiscountCouponCodesModel();
            model.DiscountId = discountId;
            return View(model);
        }
    }
}
