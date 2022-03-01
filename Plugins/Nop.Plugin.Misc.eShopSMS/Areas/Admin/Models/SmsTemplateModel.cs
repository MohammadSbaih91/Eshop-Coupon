using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.eShopSMS.Areas.Admin.Models
{
    /// <summary>
    /// Represents a SMS template model
    /// </summary>
    public partial class SmsTemplateModel : BaseNopEntityModel, ILocalizedModel<SmsTemplateLocalizedModel>
    {
        #region Ctor

        public SmsTemplateModel()
        {
            Locales = new List<SmsTemplateLocalizedModel>();
        }

        #endregion

        #region Properties
        
        [NopResourceDisplayName("Plugins.Misc.eShopSMS.Fields.TemplateName")]
        public string TemplateName { get; set; }
        
        [NopResourceDisplayName("Plugins.Misc.eShopSMS.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Plugins.Misc.eShopSMS.Fields.IsActive")]
        public bool IsActive { get; set; }
        
        public IList<SmsTemplateLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class SmsTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public SmsTemplateLocalizedModel()
        {
            
        }

        public int LanguageId { get; set; }
       
        [NopResourceDisplayName("Plugins.Misc.eShopSMS.Fields.Body")]
        public string Body { get; set; }
    }
}