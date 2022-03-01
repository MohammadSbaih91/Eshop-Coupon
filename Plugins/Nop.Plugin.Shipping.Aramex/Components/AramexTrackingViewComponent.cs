using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;

namespace Nop.Plugin.Shipping.Aramex.Components
{
    [ViewComponent(Name = "AramexTracking")]
    public class AramexTrackingViewComponent : NopViewComponent
    {
        public AramexTrackingViewComponent()
        {

        }

        public IViewComponentResult Invoke(int productId)
        {
            throw new NotImplementedException();
        }
    }
}
