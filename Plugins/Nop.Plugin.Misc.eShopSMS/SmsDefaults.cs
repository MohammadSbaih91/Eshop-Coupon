namespace Nop.Plugin.Misc.eShopSMS
{
    public static class SMSTemplateName
    {
        public static string OrderCancelled => "OrderCancelled.CustomerNotification";
        public static string OrderCompleted => "OrderCompleted.CustomerNotification";
        public static string OrderPlaced => "OrderPlaced.CustomerNotification";
        public static string OrderUncovered => "OrderUncovered.CustomerNotification";
        public static string ServiceUncovered => "ServiceUncovered.CustomerNotification";
        public static string ShipmentDelivered => "ShipmentDelivered.CustomerNotification";
    }

    public static class SmsDefaults
    {
        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static string SmsTemplatesAllCacheKey => "Nop.Sms.all";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        public static string SmsTemplatesByNameCacheKey => "Nop.Sms.name-{0}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string SmsTemplatesPatternCacheKey => "Nop.Sms.";
    }
}
