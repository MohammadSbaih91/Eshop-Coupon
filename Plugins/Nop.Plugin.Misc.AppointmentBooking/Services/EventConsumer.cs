using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Web;
using System.Linq;
using System;

namespace Nop.Plugin.Misc.AppointmentBooking.Services
{
    public class EventConsumer : IConsumer<OrderPlacedEvent>
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IAppointmentService _appointmentService;
        #endregion

        public EventConsumer(IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IOrderService orderService,
            IAppointmentService appointmentService)
        {   
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderService = orderService;
            _appointmentService = appointmentService;
        }

        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            var bookAppointmentId = _genericAttributeService.GetAttribute<int>(_workContext.CurrentCustomer, EShopHelper.BookAppintmentId, _storeContext.CurrentStore.Id);
            var order = _orderService.GetOrderById(eventMessage.Order.Id);
            if (order != null && order.PickUpInStore)
            {   
                var bookedAppointment = _appointmentService.UpdateOrderIdByBookedAppointmentId(bookAppointmentId, order.Id);
                _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, EShopHelper.BookAppintmentId, null, _storeContext.CurrentStore.Id);

                var appBookedCustomerNotificationQueuedEmailIds = _appointmentService.SendAppointmentBookedCustomerNotification(bookedAppointment, _workContext.WorkingLanguage.Id);
                if (appBookedCustomerNotificationQueuedEmailIds.Any())
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = $"\"Appointment Booked\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", appBookedCustomerNotificationQueuedEmailIds)}.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    _orderService.UpdateOrder(order);
                }
            }
        }
    }
}
