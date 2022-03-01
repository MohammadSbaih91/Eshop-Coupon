using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;
using Nop.Web.Framework.Themes;
using System.Collections.Generic;
using System.Linq;
namespace Nop.Plugin.Payments.OrangeMoney.ViewEngines
{
    public class PaymentsOrangeMoneyLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Main testimonial view on front-end side
            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                // Top mene 
                if (context.ViewName == "Components/OrderPaymentStatus/Default")
                {
                    viewLocations = new[] {
                        $"~/Plugins/Payments.OrangeMoney/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/Payments.OrangeMoney/Views/Shared/{{0}}.cshtml",
                    }
                   .Concat(viewLocations);
                }

            }
            return viewLocations;
        }
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context.AreaName?.Equals(AreaNames.Admin) ?? false)
                return;
            var themeContext = (IThemeContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(IThemeContext));
            context.Values[THEME_KEY] = themeContext.WorkingThemeName;
        }
    }
}