using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.ExternalAuth.Orange.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.ExternalAuth.Orange.OrnageURL")]
        public string OrnageURL { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.Orange.OrnageEnable")]
        public bool IsEnable { get; set; }
    }
}