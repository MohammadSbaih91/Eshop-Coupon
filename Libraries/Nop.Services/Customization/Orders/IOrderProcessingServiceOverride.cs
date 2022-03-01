using Nop.Core.Domain.Orders;

namespace Nop.Services.Customization.Orders
{
    public interface IOrderProcessingServiceOverride
    {
        /// <summary>
        /// Cancels order with reason
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        void CancelOrderWithReason(Order order, bool notifyCustomer, string cancelReason);
    }
}
