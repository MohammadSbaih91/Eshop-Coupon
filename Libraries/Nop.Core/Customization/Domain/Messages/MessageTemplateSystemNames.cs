namespace Nop.Core.Domain.Messages
{
    public static partial class MessageTemplateSystemNames
    {
        #region Order
        /// <summary>
        /// Represents system name of notification customer about Uncovered order
        /// </summary>
        public const string OrderUncoveredCustomerNotification = "OrderUncovered.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about Unreachable order
        /// </summary>
        public const string OrderUnreachableCustomerNotification = "OrderUnreachable.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about Unreachable order
        /// </summary>
        public const string OrderEmployeeDetailNotification = "OrderPlaced.EmployeeDetail";
        #endregion
    }
}
