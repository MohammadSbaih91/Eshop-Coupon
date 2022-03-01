using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.AppointmentBooking.Components
{
    [ViewComponent(Name = "WidgetsAppointmentBookingDrawer")]
    public class WidgetsAppointmentBookingDrawerViewComponent : NopViewComponent
    {
        public WidgetsAppointmentBookingDrawerViewComponent()
        {
        }

        public IViewComponentResult Invoke(string widgetZone)
        {
            return View($"~/Plugins/Misc.AppointmentBooking/Views/AppointmentBookingDrawer.cshtml");
        }
    }
}
