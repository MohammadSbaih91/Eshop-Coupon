using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Models;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    public class AppbookconfigController : BasePluginController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public AppbookconfigController(IStoreContext storeContext,
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
            var appointmentBookingSettings = _settingService.LoadSetting<AppointmentBookingSettings>(storeScope);
            var model = new ConfigurationModel
            {
                APIUrl = appointmentBookingSettings.APIUrl,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.APIUrl_OverrideForStore = _settingService.SettingExists(appointmentBookingSettings, x => x.APIUrl, storeScope);
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
            var appointmentBookingSettings = _settingService.LoadSetting<AppointmentBookingSettings>(storeScope);

            appointmentBookingSettings.APIUrl = model.APIUrl;
            
            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(appointmentBookingSettings, x => x.APIUrl, model.APIUrl_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            _settingService.ClearCache();
            
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }
        #endregion
    }
}
