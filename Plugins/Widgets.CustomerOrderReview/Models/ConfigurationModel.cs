using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Widgets.CustomerOrderReview.Models
{
    public partial class ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>, ILocalizedModel
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
        }
        
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating1")]
        public string Rating1 { get; set; }  
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating2")]
        public string Rating2 { get; set; }  
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating3")]
        public string Rating3 { get; set; }  
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating4")]
        public string Rating4 { get; set; }
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Feedback")]
        public string Feedback { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }
        
        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }
            [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating1")]
            public string Rating1 { get; set; }  
            [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating2")]
            public string Rating2 { get; set; }  
            [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating3")]
            public string Rating3 { get; set; }  
            [NopResourceDisplayName("Plugins.CustomerOrderReview.Rating4")]
            public string Rating4 { get; set; }
            [NopResourceDisplayName("Plugins.CustomerOrderReview.Feedback")]
            public string Feedback { get; set; }

        }
    }
}