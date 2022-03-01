using Nop.Core.Configuration;

namespace Nop.Web.Customization.Settings
{
    /// <summary>
    /// Tax settings
    /// </summary>
    public class EShopSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show Payments steps on one page checkout or not
        /// </summary>
        public bool HidePaymentFromOpc { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to show ShippingMethod step on one page checkout or not
        /// </summary>
        public bool HideShippingMethodFromOpc { get; set; }
    }
}