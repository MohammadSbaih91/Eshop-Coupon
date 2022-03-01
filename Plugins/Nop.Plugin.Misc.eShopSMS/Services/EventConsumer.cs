using Nop.Core.Domain.Orders;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.eShopSMS.Services
{
    public class EventConsumer : IConsumer<OrderPlacedEvent>
    {
        #region Fields
        private readonly ISendSMSNotificationService _sendSMSNotificationService;
        #endregion

        #region Ctor
        public EventConsumer(ISendSMSNotificationService sendSMSNotificationService)
        {
            _sendSMSNotificationService = sendSMSNotificationService;
        }
        #endregion
        
        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;

            _sendSMSNotificationService.SendCustomerNotification(SMSTemplateName.OrderPlaced, order);
        }

    }
}
