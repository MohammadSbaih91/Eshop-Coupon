using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Web.Framework.Themes
{
    /// <summary>
    /// Specifies the contracts for a view location expander that is used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine instances to determine search paths for a view.
    /// </summary>
    public class CustomizationViewLocationExpander : IViewLocationExpander
    {
        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine the
        /// values that would be consumed by this instance of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.
        /// The calculated values are used to determine if the view location has changed since the last time it was located.
        /// </summary>
        /// <param name="context">Context</param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine potential locations for a view.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="viewLocations">View locations</param>
        /// <returns>iew locations</returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            if (context.AreaName?.Equals(AreaNames.Admin) ?? false)
            {
                viewLocations = new[]
                    {
                        $"/Areas/{{2}}/Customization/Views/{{1}}/{{0}}.cshtml",
                        $"/Areas/{{2}}/Customization/Views/Shared/{{0}}.cshtml",
                    }
                    .Concat(viewLocations);
            }

            viewLocations = new[]
                {
                    $"/Customization/Views/{{1}}/{{0}}.cshtml",
                    $"/Customization/Views/Shared/{{0}}.cshtml",
                }
                .Concat(viewLocations);

            return viewLocations;
        }
    }
}