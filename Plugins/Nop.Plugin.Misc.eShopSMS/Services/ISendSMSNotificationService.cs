using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.eShopSMS.Services
{
    public interface ISendSMSNotificationService
    {
        void SendCustomerNotification(string sMSTemplateName, Order order);
    }
}
