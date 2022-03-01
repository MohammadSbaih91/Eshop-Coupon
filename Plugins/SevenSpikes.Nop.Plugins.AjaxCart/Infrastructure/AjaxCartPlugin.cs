using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Framework.Plugin;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Infrastructure
{
    public class AjaxCartPlugin : BaseAdminWidgetPlugin7Spikes
    {
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        private static readonly List<MenuItem7Spikes> MenuItems = new List<MenuItem7Spikes>()
        {
            new MenuItem7Spikes()
            {
                SubMenuName = "SevenSpikes.NopAjaxCart.Admin.Submenus.Settings",
                SubMenuRelativePath = "NopAjaxCartAdmin/Settings"
            }
        };

        private static bool IsTrialVersion => false;

        public AjaxCartPlugin(WidgetSettings widgetSettings, ISettingService settingService)
            : base(MenuItems,
                "SevenSpikes.Plugins.AjaxCart.Admin.Menu.MenuName",
                "SevenSpikes.Nop.Plugins.AjaxCart",
                isTrialVersion: IsTrialVersion,
                pluginUrlInStore: "http://www.nop-templates.com/ajax-cart-plugin-for-nopcommerce")
        {
            _widgetSettings = widgetSettings;
            _settingService = settingService;
        }

        public override string GetConfigurationPageUrl() => StoreLocation + "Admin/NopAjaxCartAdmin/Settings";

        public override string GetWidgetViewComponentName(string widgetZone) => "NopAjaxCart";

        protected override void InstallAdditionalSettings()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains("SevenSpikes.Nop.Plugins.AjaxCart"))
                return;
            _widgetSettings.ActiveWidgetSystemNames.Add("SevenSpikes.Nop.Plugins.AjaxCart");
            _settingService.SaveSetting(_widgetSettings, 0);
        }
    }
}