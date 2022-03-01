using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.eShopSMS.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.eShopSMS
{
    public class eShopSMSPlugin : BasePlugin, IAdminMenuPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly SMSObjectContext _sMSObjectContext;

        public eShopSMSPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            SMSObjectContext sMSObjectContext)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _sMSObjectContext = sMSObjectContext;
        }
        
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "admin/SMSConfig/Configure";
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _sMSObjectContext.Install();
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<EShopSMSSettings>();
            
            base.Uninstall();
        }

        /// <summary>
        /// Add menu in admin area base on condition if PDD is available then add chield menu in that or create PDD menu also
        /// </summary>
        /// <param name="rootNode">Root menu of admin area</param>
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "eshop.SMSTemplate",
                Title = _localizationService.GetResource("Plugins.Misc.eShopSMS.Menu.SmsTemplate"),
                ControllerName = "SMSTemplate",
                ActionName = "List",
                Visible = true,
                IconClass = "fa fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            };
            rootNode.ChildNodes.Add(menuItem);
        }
    }
}
