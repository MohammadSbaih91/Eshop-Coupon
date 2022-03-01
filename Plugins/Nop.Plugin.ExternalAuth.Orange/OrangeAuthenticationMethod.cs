using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Nop.Plugin.ExternalAuth.Facebook
{
    /// <summary>
    /// Represents method for the authentication with Facebook account
    /// </summary>
    public class OrangeAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<Language> _languagerepository;
        private readonly INopFileProvider _fileProvider;
        #endregion

        #region Ctor

        public OrangeAuthenticationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IRepository<Language> languagerepository,
            INopFileProvider fileProvider)
        {
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            _languagerepository = languagerepository;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Install resource string
        /// </summary>
        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var enLanguage = _languagerepository.Table.FirstOrDefault(l => l.LanguageCulture.ToLower() == "en-us" & l.Published);
            //save resources
            if (enLanguage != null)
            {
                foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath("~/Plugins/ExternalAuth.Orange/Localization/ResourceString"),
                "ResourceStringEn.xml", SearchOption.TopDirectoryOnly))
                {
                    var localesXml = File.ReadAllText(filePath);
                    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                    localizationService.ImportResourcesFromXml(enLanguage, localesXml);
                }
            }

        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual void DeleteLocalResources()
        {
            var file = Path.Combine(_fileProvider.MapPath("~/Plugins/ExternalAuth.Orange/Localization/ResourceString"), "ResourceStringEn.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                _localizationService.DeletePluginLocaleResource(item);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/OrangeAuthentication/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "OrangeAuthenticationComponent";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //Insert resource string
            InstallLocaleResources();
            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //Delete resource string
            DeleteLocalResources();
            base.Uninstall();
        }

        #endregion
    }
}