using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.eShopSMS.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.eShopSMS.Areas.Admin.Controllers
{
    public class SMSConfigController : BaseAdminController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public SMSConfigController(IStoreContext storeContext,
            IPermissionService permissionService,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            _storeContext = storeContext;
            _permissionService = permissionService;
            _settingService = settingService;
            _localizationService = localizationService;
        }
        #endregion

        #region Methods
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var eShopSMSSettings = _settingService.LoadSetting<EShopSMSSettings>(storeScope);
            var model = new ConfigurationModel
            {
                APIUrl = eShopSMSSettings.APIUrl,
                UserName = eShopSMSSettings.UserName,
                Password = eShopSMSSettings.Password,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.APIUrl_OverrideForStore = _settingService.SettingExists(eShopSMSSettings, x => x.APIUrl, storeScope);
                model.UserName_OverrideForStore = _settingService.SettingExists(eShopSMSSettings, x => x.UserName, storeScope);
                model.Password_OverrideForStore = _settingService.SettingExists(eShopSMSSettings, x => x.Password, storeScope);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var eShopSMSSettings = _settingService.LoadSetting<EShopSMSSettings>(storeScope);

            eShopSMSSettings.APIUrl = model.APIUrl;
            eShopSMSSettings.UserName = model.UserName;
            eShopSMSSettings.Password = model.Password;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(eShopSMSSettings, x => x.APIUrl, model.APIUrl_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(eShopSMSSettings, x => x.UserName, model.UserName_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(eShopSMSSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }
        #endregion
    }
}
