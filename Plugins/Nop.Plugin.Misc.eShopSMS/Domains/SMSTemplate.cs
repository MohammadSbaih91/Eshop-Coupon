using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Misc.eShopSMS.Domains
{
    public partial class SMSTemplate : BaseEntity, ILocalizedEntity
    {
        public string TemplateName { get; set; }

        public string Body { get; set; }

        public bool IsActive { get; set; }

        public string CreatedOn { get; set; }
    }
}
