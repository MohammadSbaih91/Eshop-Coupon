using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using System.Linq;
using Nop.Services.Common;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Nop.Core.Data;
using Nop.Web.Framework.Localization;

namespace Nop.Web.Framework.Customization.Mvc.Filters
{
    public class CustomActionFilter : ActionFilterAttribute
    {
        #region Fields
        private readonly IWebHelper _webHelper;
        #endregion

        #region Ctor
        public CustomActionFilter(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }
        #endregion

        #region Methods

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Request == null)
                return;

            //only in GET requests
            if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!DataSettingsManager.DatabaseIsInstalled)
                return;

            var pageUrl = _webHelper.GetRawUrl(context.HttpContext.Request);


            if (!pageUrl.Contains("Admin"))
            {
                if (context.RouteData.Values.ContainsKey("url") && !string.IsNullOrEmpty(context.RouteData.Values["url"].ToString()))
                {
                    pageUrl = context.RouteData.Values["url"].ToString();
                }
                var newPageUrl = UrlStrucutre.UrlDecode(pageUrl);
                if (newPageUrl.EndsWith("/en"))
                {
                    newPageUrl += "/";
                }
                if (newPageUrl != pageUrl)
                    context.Result = new RedirectResult(newPageUrl, false);
            }
        }
        #endregion
    }
}
