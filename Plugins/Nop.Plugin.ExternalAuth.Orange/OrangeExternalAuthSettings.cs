using Nop.Core.Configuration;

namespace Nop.Plugin.ExternalAuth.Orange
{
    /// <summary>
    /// Represents settings of the Orange authentication method
    /// </summary>
    public class OrangeExternalAuthSettings : ISettings
    {
        /// <summary>
        /// Gets or sets Orange URL client identifier
        /// </summary>
        public string OrnageURL { get; set; }

        public bool IsEnable { get; set; }
    }
}
