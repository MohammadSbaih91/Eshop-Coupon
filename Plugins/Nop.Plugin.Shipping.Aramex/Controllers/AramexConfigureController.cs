using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Shipping.Aramex.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Shipping.Aramex.Controllers
{
    public class AramexConfigureController : BaseAdminController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor
        public AramexConfigureController(ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._storeContext = storeContext;
        }
        #endregion

        #region Methods
        public IActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var aramexSetting = _settingService.LoadSetting<AramexSetting>(storeScope);

            var model = new AramexConfigureModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                UseSandbox = aramexSetting.UseSandbox,
                UserName = aramexSetting.UserName,
                Password = aramexSetting.Password,
                Version = aramexSetting.Version,
                AccountNumber = aramexSetting.AccountNumber,
                AccountPin = aramexSetting.AccountPin,
                AccountEntity = aramexSetting.AccountEntity,
                AccountCountryCode = aramexSetting.AccountCountryCode,
                Source = aramexSetting.Source
            };

            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.UseSandbox, storeScope);

                model.UserName_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.UserName, storeScope);
                model.Password_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.Password, storeScope);
                model.Version_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.Version, storeScope);
                model.AccountNumber_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.AccountNumber, storeScope);
                model.AccountPin_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.AccountPin, storeScope);
                model.AccountEntity_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.AccountEntity, storeScope);
                model.AccountCountryCode_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.AccountCountryCode, storeScope);
                model.Source_OverrideForStore = _settingService.SettingExists(aramexSetting, x => x.Source, storeScope);
            }
            return View(AramexDefault.ViewPath + "Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(AramexConfigureModel model)
        {

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var aramexSetting = _settingService.LoadSetting<AramexSetting>(storeScope);

            //save settings

            aramexSetting.UseSandbox = model.UseSandbox;
            aramexSetting.UserName = model.UserName;
            aramexSetting.Password = model.Password;
            aramexSetting.Version = model.Version;
            aramexSetting.AccountNumber = model.AccountNumber;
            aramexSetting.AccountPin = model.AccountPin;
            aramexSetting.AccountEntity = model.AccountEntity;
            aramexSetting.AccountCountryCode = model.AccountCountryCode;
            aramexSetting.Source = model.Source;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.UserName, model.UserName_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.Version, model.Version_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.AccountNumber, model.AccountNumber_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.AccountPin, model.AccountPin_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.AccountEntity, model.AccountEntity_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.AccountCountryCode, model.AccountCountryCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aramexSetting, x => x.Source, model.Source_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
        #endregion
    }
}
