using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Misc.AppointmentBooking.Models;
using Nop.Plugin.Misc.AppointmentBooking.Services;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Checkout;
using System;

namespace Nop.Plugin.Misc.AppointmentBooking.Components
{
    [ViewComponent(Name = "CompleteTicket")]
    public class CompleteTicketViewComponent : NopViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly IAppointmentService _appointmentService;

        public CompleteTicketViewComponent(IOrderService orderService,
            IAppointmentService appointmentService)
        {
            _orderService = orderService;
            _appointmentService = appointmentService;
        }

        public IViewComponentResult Invoke(string widgetZone, int orderId)
        {
            var appointment = _appointmentService.GetBookedAppointmentByOrderId(orderId);
            if (appointment == null)
                return Content("");

            var model = new ConfirmAppointment()
            {
                BookAppointmentRequest = new BookAppointmentRequest()
                {
                    OrderId = appointment.OrderId,
                    AppointmentDay = appointment.AppointmentDay,
                    SelectedAppointmentTime = appointment.AppointmentTime,
                    BranchID = appointment.BranchID,
                    ServiceID = appointment.ServiceID,
                    x_wassup_msisdn = appointment.x_wassup_msisdn,
                    SelectedStoreName = appointment.SelectedStoreName
                },
                BookAppointmentResponse = JsonConvert.DeserializeObject<BookAppointmentResponse>(appointment.JsonResponce)
            };

            return View($"~/Plugins/Misc.AppointmentBooking/Views/CompleteTicket.cshtml", model);
        }
    }
}
