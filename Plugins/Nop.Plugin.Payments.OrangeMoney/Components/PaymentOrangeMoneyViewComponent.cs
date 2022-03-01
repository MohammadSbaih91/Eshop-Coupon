using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.OrangeMoney.Components
{
    [ViewComponent(Name = "PaymentOrangeMoney")]
    public class PaymentOrangeMoneyViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.OrangeMoney/Views/PaymentInfo.cshtml");
        }
    }
}
