using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Widgets.AnywhereSlider.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AnywhereSlider
{
    public class AnywhereSliderPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Language> _languageRepository;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly INopFileProvider _fileProvider;
        private readonly AnywhereSliderObjectContext _objectContext;

        public AnywhereSliderPlugin(ILocalizationService localizationService,
            IRepository<Language> languageRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            INopFileProvider fileProvider,
            AnywhereSliderObjectContext objectContext)
        {
            _localizationService = localizationService;
            _languageRepository = languageRepository;
            _settingService = settingService;
            _webHelper = webHelper;
            _fileProvider = fileProvider;
            _objectContext = objectContext;
        }
        
        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                "home_page_top",
                "home_page_bottom",
                "categorydetails_bottom",
                "categorydetails_top",
                "categorydetails_top_MobilePlans",
                "categorydetails_top_Internet",
                "categorydetails_top_SmartLife",
                "categorydetails_top_FixedLine",
            };
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsAnywhereSlider";
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //database objects
            //_objectContext.Install();

            base.Install();
        }


        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }

        /// <summary>
        /// Add menu in admin area base on condition if GBS is available then add chield menu in that or create GBS menu also
        /// </summary>
        /// <param name="rootNode">Root menu of admin area</param>
        public void ManageSiteMap(SiteMapNode rootNode)
        {

            var anywhereSlider = new SiteMapNode()
            {
                SystemName = "plugin.AnywhereSlider",
                Title = _localizationService.GetResource("Widgets.AnywhereSlider"),
                ControllerName = "AnywhereSlider",
                ActionName = "List",
                Visible = true,
                IconClass = "fa fa fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            };
            
            rootNode.ChildNodes.Add(anywhereSlider);

        }
    }
}
