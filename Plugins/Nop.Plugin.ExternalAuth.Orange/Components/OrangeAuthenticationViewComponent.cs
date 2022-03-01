using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.ExternalAuth.Orange.Models;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.ExternalAuth.Orange.Components
{
    [ViewComponent(Name = "OrangeAuthenticationComponent")]
    public class OrangeAuthenticationViewComponent : NopViewComponent
    {
        #region Fields
        private readonly OrangeExternalAuthSettings _orangeExternalAuthSettings;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        public OrangeAuthenticationViewComponent(ISettingService settingService,
            OrangeExternalAuthSettings orangeExternalAuthSettings,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _settingService = settingService;
            _orangeExternalAuthSettings = orangeExternalAuthSettings;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext= workContext;
        }
        #endregion

        #region Methods
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (_orangeExternalAuthSettings.IsEnable)
            {
                var lang = _workContext.WorkingLanguage.UniqueSeoCode;
                var redirectUrl = "";
                if (!string.IsNullOrEmpty(HttpContext.Request.Query["returnUrl"]))
                    redirectUrl = HttpContext.Request.Query["returnUrl"];

                var orangeUrl = _orangeExternalAuthSettings.OrnageURL.ToLower().Replace("{lanseo}", lang);

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    orangeUrl = orangeUrl.Replace("{lanseo}", "");
                }
                redirectUrl = redirectUrl.Replace("/ar/", "/");
                redirectUrl = redirectUrl.Replace("/en/", "/");
                var model = new ExternalAuthOrangeModel()
                {
                    
                    OrnageURL = orangeUrl + redirectUrl,
                };

                return View("~/Plugins/ExternalAuth.Orange/Views/ExternalAuth.OrangeLoginButton.cshtml", model);

            }
            return Content("");
        }
        #endregion
    }
}
