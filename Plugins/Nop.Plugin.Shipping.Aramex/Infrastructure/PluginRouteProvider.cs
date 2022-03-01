using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;
using System.Linq;

namespace Nop.Plugin.Shipping.Aramex.Infrastructure
{
    public partial class PluginRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapLocalizedRoute("AramexTrackingInfo", "order/aramextrackyourorder",
                new { controller = "AramexTracking", action = "AramexTrackYourOrder" });

            routeBuilder.MapLocalizedRoute("AramexOrderTracking", "order/aramexordertracking/{id:regex(\\d*)}",
                new { controller = "AramexTracking", action = "AramexOrderTracking" });

            routeBuilder.MapLocalizedRoute("AramexOrderActivity", "order/aramexorderactivity",
                new { controller = "AramexTracking", action = "AramexOrderActivity" });

            routeBuilder.MapLocalizedRoute("AramexOrderCancelled", "order/ordercancelled",
                new { controller = "AramexTracking", action = "AramexOrderCancelled" });
        }

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return 31; }
        }

        #endregion
    }
}
