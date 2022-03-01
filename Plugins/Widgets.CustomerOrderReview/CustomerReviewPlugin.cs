using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;
using Widgets.CustomerOrderReview.Data;

namespace Widgets.CustomerOrderReview
{
    /// <summary>
    /// Plugin
    /// </summary>
    public class CustomerOrderReviewPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly CustomerOrderReviewObjectContext _objectContext;

        public CustomerOrderReviewPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper, CustomerOrderReviewObjectContext objectContext)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _objectContext = objectContext;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { PublicWidgetZones.CheckoutCompletedBottom };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/CustomerOrderReview/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "CustomerOrderReview";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _objectContext.Install();
        
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Headline", "Set Review Questions");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Title", "Let us know your experience");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Submit.Success", "Your review has been saved, Thank you.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Submit.Fail", "Something went wrong");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Rating1", "Question1");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Rating2", "Question2");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Rating3", "Question3");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Rating4", "Question4");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Feedback", "Descriptive");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReviewType.Negative", "Negative");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReviewType.Neutral", "Neutral");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReviewType.Positive", "Positive");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.CreatedOnFrom", "From Date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.CreatedOnTo", "To Date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.SearchText", "Feedback");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Order", "Order No");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.Customer", "Customer");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.CreatedOn", "Created On");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.SearchOrder", "Order No");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.CustomerOrderReview.CustomerOrderReviewType", "Review Type");
           
            //settings
            var settings = new CustomerOrderReviewSettings
            {
                Rate1Label = "Accurate Description",
                Rate2Label = "Seller's Communication",
                Rate3Label = "Shipping Speed",
                Rate4Label = "Reasonable Shipping Cost",
                FeedbackLabel = "Enter Feedback"
            };
            _settingService.SaveSetting(settings);
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            _objectContext.Uninstall();

            //settings
            _settingService.DeleteSetting<CustomerOrderReviewSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Headline");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Title");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Submit.Success");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Submit.Fail");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Rating1");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Rating2");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Rating3");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Rating4");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Feedback");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReviewType.Negative");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReviewType.Neutral");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReviewType.Positive");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.CreatedOnFrom");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.CreatedOnTo");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.SearchText");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Order");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.Customer");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.CreatedOn");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.SearchOrder");
            _localizationService.DeletePluginLocaleResource("Plugins.CustomerOrderReview.CustomerOrderReviewType");
            base.Uninstall();
        }
    }
}