using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.eShopSMS
{
    public class EShopSMSSettings : ISettings
    {
        public string APIUrl { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
