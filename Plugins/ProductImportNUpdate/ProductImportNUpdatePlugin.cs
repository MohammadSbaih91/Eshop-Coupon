using System;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using ProductImportNUpdate.Infrastructure;

namespace ProductImportNUpdate
{
    /// <summary>
    /// Plugin
    /// </summary>
    public class ProductImportNUpdatePlugin : BasePlugin, IMiscPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly INopFileProvider _fileProvider;
        private readonly IWebHelper _webHelper;

        public ProductImportNUpdatePlugin(
            ILocalizationService localizationService,
            IWebHelper webHelper, 
            ISettingService settingService, INopFileProvider fileProvider)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _fileProvider = fileProvider;
        }


        private void EnsureFolderExists(string globalPath)
        {
            try
            {
                _fileProvider.CreateDirectory(_fileProvider.Combine(globalPath, "Logs"));
                _fileProvider.CreateDirectory(_fileProvider.Combine(globalPath, "ToImport"));
                _fileProvider.CreateDirectory(_fileProvider.Combine(globalPath, "Imported"));
            }
            catch (Exception)
            {
            }
        }
        
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ProductImportNUpdate/Configure";
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("Plugin.ProductImportNUpdate.UnsupportedFile", "Unsupported or corrupted file");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugin.ProductImportNUpdate.submit", "Import");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugin.ProductImportNUpdate.DragDrop", "You can chose by clicking or just drag and drop your file here");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugin.ProductImportNUpdate.File", "Upload .csv file with (,) separator");
            var settings = new ProductImportNUpdateSettings { GlobalPath = "C:/ProductImportNUpdate/",EnableAutoImport = true, Separator = ';'};
            _settingService.SaveSetting(settings);
            EnsureFolderExists(settings.GlobalPath);
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugin.ProductImportNUpdate.UnsupportedFile");
            _localizationService.DeletePluginLocaleResource("Plugin.ProductImportNUpdate.submit");
            _localizationService.DeletePluginLocaleResource("Plugin.ProductImportNUpdate.DragDrop");
            _localizationService.DeletePluginLocaleResource("Plugin.ProductImportNUpdate.File");
            _settingService.DeleteSetting<ProductImportNUpdateSettings>();
            base.Uninstall();
        }
    }
}