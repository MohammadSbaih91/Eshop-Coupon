using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Eskadenia
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
            routeBuilder.MapRoute("Plugin.Payments.Eskadenia.PaymentResponse", "PaymentEskadenia/PaymentResponse",
                 new { controller = "PaymentEskadenia", action = "PaymentResponse" }); 
            
            //order re-payment
            routeBuilder.MapRoute("RetryPayment", "order/repayment/{orderId:min(0)}",
                 new { controller = "PaymentEskadenia", action = "PaymentMethod" });
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
