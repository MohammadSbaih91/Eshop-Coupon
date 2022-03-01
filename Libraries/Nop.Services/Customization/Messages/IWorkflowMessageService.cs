using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Employees;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial interface IWorkflowMessageService
    {
        #region Send a message to a friend
        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <param name="customerFullName">Customer Full Name</param>
        /// <returns>Queued email identifier</returns>
        IList<int> SendProductEmailAFriendMessage(Customer customer, int languageId,
            Product product, string customerEmail, string friendsEmail, string personalMessage,
            string customerFullName);
        #endregion

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<int> SendOrderCancelledCustomerNotificationWithReason(Order order, int languageId, string cancelReason);

        /// <summary>
        /// Sends an order Uncovered notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<int> SendOrderUncoveredCustomerNotification(Order order, int languageId);

        /// <summary>
        /// Sends an order Unreachable notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<int> SendOrderUnreachableCustomerNotification(Order order, int languageId);

        /// <summary>
        /// Sends employe notification
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<int> SendOrderEmployeeNotification(Order order, int languageId, Employee employee);
    }
}