using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Aramex
{
    public class AramexSetting : ISettings
    {
        public bool UseSandbox { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Version { get; set; }
        public string AccountNumber { get; set; }
        public string AccountPin { get; set; }
        public string AccountEntity { get; set; }
        public string AccountCountryCode { get; set; }
        public string Source { get; set; }
    }
}
