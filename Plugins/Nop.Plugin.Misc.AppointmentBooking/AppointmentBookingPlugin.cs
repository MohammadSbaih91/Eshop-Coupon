using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.AppointmentBooking.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.AppointmentBooking
{
    public class AppointmentBookingPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly AppointmentBookingObjectContext _appointmentBookingObjectContext;

        public AppointmentBookingPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            AppointmentBookingObjectContext appointmentBookingObjectContext)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _appointmentBookingObjectContext = appointmentBookingObjectContext;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            //return new List<string> { PublicWidgetZones.CheckoutCompletedTop };
            // now (new theme) Didnt required to show a booking button
            return new List<string> { };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/Appbookconfig/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsAppointmentBooking";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _appointmentBookingObjectContext.Install();
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<AppointmentBookingSettings>();
            
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
                SystemName = "eshop.AppointmentBranch",
                Title = _localizationService.GetResource("Plugins.Misc.AppointmentBooking.AppointmentBranch.menu"),
                ControllerName = "AppointmentBranch",
                ActionName = "List",
                Visible = true,
                IconClass = "fa fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            };
            rootNode.ChildNodes.Add(menuItem);
        }
    }
}
