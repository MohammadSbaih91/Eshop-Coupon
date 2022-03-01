using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models
{
    public class SliderSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Widgets.AnywhereSlider.SearchName")]
        public string SearchName { get; set; }
    }
}
