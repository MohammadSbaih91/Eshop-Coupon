using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.OrangeMoney
{
    /// <summary>
    /// Represents settings of the OrangeMoney payment plugin
    /// </summary>
    public class OrangeMoneyPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a merchant code
        /// </summary>
        public string MerchantCode { get; set; }

        /// <summary>
        /// Gets or sets channel Key
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// Gets or API End Key
        /// </summary>
        public string APIEndPoint { get; set; }

        /// <summary>
        /// Gets or PSP
        /// </summary>
        public string PSP { get; set; }
    }
}
