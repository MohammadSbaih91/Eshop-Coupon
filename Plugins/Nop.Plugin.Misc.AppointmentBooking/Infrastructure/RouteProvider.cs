using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AppointmentBooking.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //add route for the access token callback
            routeBuilder.MapLocalizedRoute("appointmentbooking.searchcity", "appointmentbooking/searchcity/", 
                new { controller = "AppointmentBooking", action = "SearchCity" });

            //add route for the access token callback
            routeBuilder.MapLocalizedRoute("appointmentbooking.Appointment", "appointmentbooking/Appointment/{orderid}/{isChangeAppointent}",
                new { controller = "AppointmentBooking", action = "Appointment" });
            
            routeBuilder.MapLocalizedRoute("appointmentbooking.AppointmentStoreList", "appointmentbooking/appointmentstorelist",
                new { controller = "AppointmentBooking", action = "AppointmentStoreList" });

            routeBuilder.MapLocalizedRoute("appointmentbooking.AppointmentStoreDetail", "appointmentbooking/appointmentstoredetail",
                new { controller = "AppointmentBooking", action = "AppointmentStoreDetail" });

            routeBuilder.MapLocalizedRoute("appointmentbooking.AppointmentTime", "appointmentbooking/appointmenttime",
                new { controller = "AppointmentBooking", action = "AppointmentTime" });

            routeBuilder.MapLocalizedRoute("appointmentbooking.BookAppointmentCompleted", "appointmentbooking/completed",
                new { controller = "AppointmentBooking", action = "BookAppointmentCompleted" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return 1; }
        }
    }
}