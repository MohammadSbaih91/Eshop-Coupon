using Microsoft.AspNetCore.Mvc;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Checkout;
using System;

namespace Nop.Plugin.Misc.AppointmentBooking.Components
{
    [ViewComponent(Name = "WidgetsAppointmentBooking")]
    public class WidgetsAppointmentBookingViewComponent : NopViewComponent
    {
        private readonly IOrderService _orderService;

        public WidgetsAppointmentBookingViewComponent(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var checkoutCompletedModel = (CheckoutCompletedModel)additionalData;
            var order = _orderService.GetOrderById(checkoutCompletedModel.OrderId);

            if (order.PickUpInStore)
                return View($"~/Plugins/Misc.AppointmentBooking/Views/AppointmentBooking.cshtml", checkoutCompletedModel.OrderId);
            else
                return Content("");
        }
    }
}
