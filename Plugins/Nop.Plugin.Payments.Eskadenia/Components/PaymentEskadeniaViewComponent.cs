using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Eskadenia.Components
{
    [ViewComponent(Name = "PaymentEskadenia")]
    public class PaymentEskadeniaViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.Eskadenia/Views/PaymentInfo.cshtml");
        }
    }
}
