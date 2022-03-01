using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.OrangeMoney
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Payment response
            routeBuilder.MapRoute("Plugin.Payments.OrangeMoney.PaymentResponse", "PaymentOrangeMoney/PaymentResponse",
                 new { controller = "PaymentOrangeMoney", action = "PaymentResponse" }); 
            
            //order re-payment
            routeBuilder.MapRoute("RetryOnlinePayment", "order/repayment/{orderId:min(0)}",
                 new { controller = "PaymentOrangeMoney", action = "PaymentMethod" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return -1; }
        }
    }
}
